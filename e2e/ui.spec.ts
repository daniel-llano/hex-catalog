import { test, expect, type Page } from "playwright/test";

const admin = { username: "catalogadmin", password: "Admin@1234" };

async function signIn(page: Page, username: string, password: string) {
  await page.goto("/account/login");
  await page.locator("#login-username").fill(username);
  await page.locator("#login-password").fill(password);
  await page.getByRole("button", { name: "Sign in" }).click();
}

async function waitForCircuit(page: Page) {
  await page.waitForFunction(() => (window as any).Blazor !== undefined);
  await page.waitForLoadState("networkidle");
}

test.describe("HexCatalog admin", () => {
  test("signed-out home offers sign in and registration", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("heading", { name: "HexCatalog admin" })).toBeVisible();
    await expect(page.getByRole("main").getByRole("link", { name: "Sign in" })).toBeVisible();
    await expect(page.getByRole("main").getByRole("link", { name: "Create account" })).toBeVisible();
  });

  test("products redirects guests to sign in", async ({ page }) => {
    await page.goto("/products");
    await expect(page).toHaveURL(/account\/login\?returnUrl=%2Fproducts/i);
    await expect(page.getByRole("heading", { name: "Sign in" })).toBeVisible();
  });

  test("wrong password shows an error", async ({ page }) => {
    await signIn(page, admin.username, "nope");
    await expect(page.getByRole("alert")).toContainText("Wrong username or password");
  });

  test("admin can create, reserve, and delete a product", async ({ page }) => {
    const name = `E2E Lamp ${Date.now()}`;
    await signIn(page, admin.username, admin.password);
    await expect(page).toHaveURL(/\/products$/);
    await expect(page.getByRole("link", { name: "Hex Laptop" })).toBeVisible();
    await waitForCircuit(page);

    await page.getByRole("link", { name: "New product" }).click();
    await expect(page.getByRole("heading", { name: "New product" })).toBeVisible();
    await page.locator("#product-name").fill(name);
    await page.locator("#product-description").fill("Desk lamp with a warm LED.");
    await page.locator("#product-price").fill("39.90");
    await page.locator("#product-stock").fill("4");
    await page.locator("#product-category").fill("Lighting");
    await page.getByRole("button", { name: "Create product" }).click();

    await expect(page.getByRole("heading", { name })).toBeVisible();
    await page.getByRole("button", { name: "Reserve stock" }).click();
    await page.locator("#reserve-qty").fill("3");
    await page.getByRole("button", { name: "Reserve", exact: true }).click();
    await expect(page.locator("dd").filter({ hasText: /^1$/ })).toBeVisible();

    await page.getByRole("link", { name: "Back to products" }).click();
    await page.getByPlaceholder("Search by name or category").fill(name);
    await page.getByRole("button", { name: "Search" }).click();
    await expect(page.getByText("1 product(s)")).toBeVisible();
    const row = page.getByRole("row", { name: new RegExp(name) });
    await row.getByRole("button", { name: "Delete" }).click();
    await page.getByRole("dialog").getByRole("button", { name: "Delete" }).click();
    await expect(page.getByText(`No products match "${name}"`)).toBeVisible();
  });

  test("new accounts wait for approval", async ({ page }) => {
    const username = `viewer${Date.now()}`;
    await page.goto("/account/register");
    await page.locator("#register-username").fill(username);
    await page.locator("#register-password").fill("Viewer@1234");
    await page.locator("#register-confirm").fill("Viewer@1234");
    await page.getByRole("button", { name: "Create account" }).click();
    await expect(page.getByText("Your account is waiting for approval.")).toBeVisible();

    await page.goto("/products");
    await expect(page.getByText("Your account is waiting for approval.")).toBeVisible();
  });
});
