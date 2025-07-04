Describe 'Get-AzToken Interactive Tests' -Tag 'Interactive' {
    BeforeDiscovery {
        $CommonTestCases = . "$BuildRoot\tests\cases\TestCases.ps1"

        $TestCases = $CommonTestCases
    }

    BeforeAll {
        # These tests require user interaction and should only be run manually
        Write-Warning "These tests require user interaction and will prompt for authentication."
        Write-Warning "Press Ctrl+C to cancel if you don't want to authenticate interactively."
    }

    Context 'Interactive Authentication' -ForEach $TestCases {
        It 'should authenticate interactively and return a valid token' {
            Write-Host "This test will open a browser window for authentication..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -Interactive -Scope $TestScope
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.Scopes | Should -Contain $TestScope[0]
            $Token.Username | Should -Not -BeNullOrEmpty
            $Token.TenantId | Should -Not -BeNullOrEmpty
        }

        It 'should authenticate interactively with custom resource' {
            Write-Host "This test will open a browser window for authentication with custom resource..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -Interactive -Resource $TestResource
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.Username | Should -Not -BeNullOrEmpty
            $Token.TenantId | Should -Not -BeNullOrEmpty
        }

        It 'should authenticate interactively with specific tenant' {
            Write-Host "This test will prompt for tenant ID and then open a browser window..." -ForegroundColor Yellow
            
            # Prompt for tenant ID for testing
            $TenantId = Read-Host "Enter a tenant ID for testing (or press Enter to skip this test)"
            
            if ([string]::IsNullOrWhiteSpace($TenantId)) {
                Set-ItResult -Skipped -Because "No tenant ID provided for testing"
                return
            }
            
            $Token = Get-AzToken -Interactive -Tenant $TenantId -Scope $TestScope
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.TenantId | Should -Be $TenantId
        }
    }

    Context 'Device Code Authentication' {
        It 'should authenticate with device code and return a valid token' {
            Write-Host "This test will display a device code for authentication..." -ForegroundColor Yellow
            Write-Host "You will need to open a browser and enter the provided code." -ForegroundColor Yellow
            
            $Token = Get-AzToken -DeviceCode -Scope $TestScope
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.Scopes | Should -Contain $TestScope[0]
            $Token.Username | Should -Not -BeNullOrEmpty
            $Token.TenantId | Should -Not -BeNullOrEmpty
        }

        It 'should authenticate with device code using custom resource' {
            Write-Host "This test will display a device code for authentication with custom resource..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -DeviceCode -Resource $TestResource
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.Username | Should -Not -BeNullOrEmpty
            $Token.TenantId | Should -Not -BeNullOrEmpty
        }

        It 'should authenticate with device code for specific tenant' {
            Write-Host "This test will prompt for tenant ID and then display a device code..." -ForegroundColor Yellow
            
            # Prompt for tenant ID for testing
            $TenantId = Read-Host "Enter a tenant ID for device code testing (or press Enter to skip this test)"
            
            if ([string]::IsNullOrWhiteSpace($TenantId)) {
                Set-ItResult -Skipped -Because "No tenant ID provided for testing"
                return
            }
            
            $Token = Get-AzToken -DeviceCode -Tenant $TenantId -Scope $TestScope
            
            # Verify token structure
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            $Token.ExpiresOn | Should -BeGreaterThan (Get-Date)
            $Token.TenantId | Should -Be $TenantId
        }
    }

    Context 'Cache Integration with Interactive Authentication' {
        BeforeAll {
            $TestCacheName = "InteractiveTestCache_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
        }

        AfterAll {
            # Cleanup test cache
            try {
                Clear-AzTokenCache -TokenCache $TestCacheName -ErrorAction SilentlyContinue
            }
            catch {
                Write-Warning "Could not clean up test cache '$($TestCacheName)': $($_.Exception.Message)"
            }
        }

        It 'should create cache during interactive authentication' {
            Write-Host "This test will authenticate interactively and create a cache..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -Interactive -TokenCache $TestCacheName -Scope $TestScope
            
            # Verify token
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            
            # Verify cache was created by trying to get token from cache
            $CachedToken = Get-AzToken -TokenCache $TestCacheName -Username $Token.Username -Scope $TestScope
            $CachedToken | Should -Not -BeNullOrEmpty
            $CachedToken.Username | Should -Be $Token.Username
        }

        It 'should create cache during device code authentication' {
            Write-Host "This test will authenticate with device code and create a cache..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -DeviceCode -TokenCache $TestCacheName -Scope $TestScope
            
            # Verify token
            $Token | Should -Not -BeNullOrEmpty
            $Token.AccessToken | Should -Not -BeNullOrEmpty
            
            # Verify cache was created by trying to get token from cache
            $CachedToken = Get-AzToken -TokenCache $TestCacheName -Username $Token.Username -Scope $TestScope
            $CachedToken | Should -Not -BeNullOrEmpty
            $CachedToken.Username | Should -Be $Token.Username
        }
    }

    Context 'Token Properties Validation' {
        It 'should return token with valid claims from interactive auth' {
            Write-Host "This test will authenticate interactively to validate token claims..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -Interactive -Scope $TestScope
            
            # Verify token has expected properties
            $Token.PSObject.Properties.Name | Should -Contain 'AccessToken'
            $Token.PSObject.Properties.Name | Should -Contain 'Scopes'
            $Token.PSObject.Properties.Name | Should -Contain 'ExpiresOn'
            $Token.PSObject.Properties.Name | Should -Contain 'Claims'
            $Token.PSObject.Properties.Name | Should -Contain 'Username'
            $Token.PSObject.Properties.Name | Should -Contain 'TenantId'
            
            # Verify claims exist and contain expected values
            $Token.Claims | Should -Not -BeNullOrEmpty
            $Token.Claims.Count | Should -BeGreaterThan 0
        }

        It 'should return token with valid claims from device code auth' {
            Write-Host "This test will authenticate with device code to validate token claims..." -ForegroundColor Yellow
            
            $Token = Get-AzToken -DeviceCode -Scope $TestScope
            
            # Verify token has expected properties
            $Token.PSObject.Properties.Name | Should -Contain 'AccessToken'
            $Token.PSObject.Properties.Name | Should -Contain 'Scopes'
            $Token.PSObject.Properties.Name | Should -Contain 'ExpiresOn'
            $Token.PSObject.Properties.Name | Should -Contain 'Claims'
            $Token.PSObject.Properties.Name | Should -Contain 'Username'
            $Token.PSObject.Properties.Name | Should -Contain 'TenantId'
            
            # Verify claims exist and contain expected values
            $Token.Claims | Should -Not -BeNullOrEmpty
            $Token.Claims.Count | Should -BeGreaterThan 0
        }
    }
}
