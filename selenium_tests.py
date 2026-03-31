"""
Item Processor App – Selenium Automation Tests
=============================================
Requirements:
    pip install selenium pytest webdriver-manager

Usage:
    pytest selenium_tests.py -v

The app must be running at http://localhost:5000 before running tests.
"""

import pytest
import time
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait, Select
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager

BASE_URL = "http://err.runasp.net"
ADMIN_EMAIL = "admin@test.com"
ADMIN_PASSWORD = "Admin@123"


# ─── Fixtures ─────────────────────────────────────────────────────────────────

@pytest.fixture(scope="session")
def driver():
    """Create a single browser session for all tests."""
    options = webdriver.ChromeOptions()
    options.add_argument("--start-maximized")
    options.add_argument("--headless")  # Enable headless mode
    svc = Service(ChromeDriverManager().install())
    d = webdriver.Chrome(service=svc, options=options)
    d.implicitly_wait(5)
    yield d
    d.quit()


@pytest.fixture
def wait(driver):
    return WebDriverWait(driver, 10)


def login(driver, email=ADMIN_EMAIL, password=ADMIN_PASSWORD):
    driver.get(f"{BASE_URL}/Auth/Login")
    driver.find_element(By.ID, "Email").clear()
    driver.find_element(By.ID, "Email").send_keys(email)
    driver.find_element(By.ID, "Password").clear()
    driver.find_element(By.ID, "Password").send_keys(password)
    driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
    time.sleep(1)


# ─── Module 1: Login ──────────────────────────────────────────────────────────

class TestLogin:

    def test_TC01_valid_login(self, driver, wait):
        """TC01: Valid login redirects to Items page."""
        login(driver)
        wait.until(EC.url_contains("/Items"))
        assert "/Items" in driver.current_url

    def test_TC02_wrong_password(self, driver):
        """TC02: Wrong password shows error."""
        login(driver, password="wrongpass")
        error = driver.find_element(By.CSS_SELECTOR, ".validation-summary")
        assert "Invalid email or password" in error.text

    def test_TC03_nonexistent_email(self, driver):
        """TC03: Non-existent email shows error."""
        login(driver, email="nobody@test.com")
        error = driver.find_element(By.CSS_SELECTOR, ".validation-summary")
        assert "Invalid email or password" in error.text

    def test_TC04_empty_email(self, driver):
        """TC04: Empty email shows validation error."""
        driver.get(f"{BASE_URL}/Auth/Login")
        driver.find_element(By.ID, "Password").send_keys("Admin@123")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        page = driver.page_source
        assert "Email is required" in page or "required" in page.lower()

    def test_TC05_empty_password(self, driver):
        """TC05: Empty password shows validation error."""
        driver.get(f"{BASE_URL}/Auth/Login")
        driver.find_element(By.ID, "Email").send_keys("admin@test.com")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        page = driver.page_source
        assert "Password is required" in page or "required" in page.lower()

    def test_TC10_logout(self, driver, wait):
        """TC10: Logout clears session and returns to Login."""
        login(driver)
        wait.until(EC.url_contains("/Items"))
        driver.get(f"{BASE_URL}/Auth/Logout")
        wait.until(EC.url_contains("/Auth/Login"))
        assert "Login" in driver.title or "/Auth/Login" in driver.current_url

    def test_TC11_protected_page_redirects(self, driver, wait):
        """TC11: Accessing protected page without login redirects."""
        # Already logged out from TC10
        driver.get(f"{BASE_URL}/Items")
        wait.until(EC.url_contains("/Auth/Login"))
        assert "/Auth/Login" in driver.current_url


# ─── Module 2: Items CRUD ─────────────────────────────────────────────────────

class TestItems:

    @pytest.fixture(autouse=True)
    def ensure_logged_in(self, driver, wait):
        """Log in before each Items test."""
        login(driver)
        wait.until(EC.url_contains("/Items"))

    def test_TC12_add_valid_item(self, driver, wait):
        """TC12: Valid item is added successfully."""
        driver.get(f"{BASE_URL}/Items/Create")
        driver.find_element(By.ID, "Name").send_keys("Selenium Test Item")
        driver.find_element(By.ID, "Weight").send_keys("99.5")
        driver.find_element(By.ID, "Description").send_keys("Created by Selenium")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        wait.until(EC.url_contains("/Items"))
        page = driver.page_source
        assert "Selenium Test Item" in page

    def test_TC13_empty_name(self, driver):
        """TC13: Empty name shows validation error."""
        driver.get(f"{BASE_URL}/Items/Create")
        driver.find_element(By.ID, "Weight").send_keys("50")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        page = driver.page_source
        assert "Item name is required" in page

    def test_TC16_negative_weight(self, driver):
        """TC16: Negative weight shows validation error."""
        driver.get(f"{BASE_URL}/Items/Create")
        driver.find_element(By.ID, "Name").send_keys("Bad Item")
        driver.find_element(By.ID, "Weight").send_keys("-5")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        page = driver.page_source
        assert "greater than 0" in page

    def test_TC29_search_by_name(self, driver, wait):
        """TC29: Search filters by item name."""
        driver.get(f"{BASE_URL}/Items?search=Selenium")
        time.sleep(0.5)
        page = driver.page_source
        assert "Selenium Test Item" in page

    def test_TC31_search_no_results(self, driver):
        """TC31: Search with no match shows empty state."""
        driver.get(f"{BASE_URL}/Items?search=XYZXYZ_NOMATCH_999")
        page = driver.page_source
        assert "No items found" in page


# ─── Module 3: Process Item ───────────────────────────────────────────────────

class TestProcessItem:

    @pytest.fixture(autouse=True)
    def ensure_logged_in(self, driver, wait):
        login(driver)
        wait.until(EC.url_contains("/Items"))

    def test_TC34_valid_process(self, driver, wait):
        """TC34: Valid process with 1 child item succeeds."""
        driver.get(f"{BASE_URL}/Process/Create")
        # Select parent item (first available)
        parent_select = Select(driver.find_element(By.ID, "ParentItemId"))
        if len(parent_select.options) < 2:
            pytest.skip("No items in database to process")
        parent_select.select_by_index(1)

        driver.find_element(By.ID, "ParentOutputWeight").send_keys("450")

        # Fill child item
        child_select = Select(driver.find_element(By.CSS_SELECTOR, "select[name='Children[0].ItemId']"))
        child_select.select_by_index(1)
        driver.find_element(By.CSS_SELECTOR, "input[name='Children[0].OutputWeight']").send_keys("50")

        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        wait.until(EC.url_contains("/Process/Tree"))
        assert "Tree" in driver.page_source or "🌳" in driver.page_source

    def test_TC35_no_parent_selected(self, driver):
        """TC35: Submitting without parent selection shows error."""
        driver.get(f"{BASE_URL}/Process/Create")
        driver.find_element(By.ID, "ParentOutputWeight").send_keys("100")
        driver.find_element(By.CSS_SELECTOR, "button[type=submit]").click()
        page = driver.page_source
        assert "select a parent item" in page.lower() or "required" in page.lower()

    def test_TC40_add_multiple_children(self, driver, wait):
        """TC40: Add Another Output button adds extra rows."""
        driver.get(f"{BASE_URL}/Process/Create")
        initial_count = len(driver.find_elements(By.CSS_SELECTOR, ".child-item"))
        add_btn = driver.find_element(By.ID, "addChildBtn")
        add_btn.click()
        time.sleep(0.3)
        add_btn.click()
        time.sleep(0.3)
        new_count = len(driver.find_elements(By.CSS_SELECTOR, ".child-item"))
        assert new_count == initial_count + 2

    def test_TC44_tree_view_loads(self, driver, wait):
        """TC44: Tree view page loads."""
        driver.get(f"{BASE_URL}/Process/Tree")
        time.sleep(0.5)
        page = driver.page_source
        # Should show either no processed items or a tree
        assert "Processed Items Tree" in page or "tree" in page.lower()


# ─── Entry point ──────────────────────────────────────────────────────────────

if __name__ == "__main__":
    pytest.main([__file__, "-v", "--tb=short"])
