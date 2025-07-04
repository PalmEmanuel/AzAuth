Describe 'Get-AzTokenCache' {
    BeforeAll {
        # Create a temporary test cache directory structure
        $TestCacheRoot = Join-Path $TestDrive '.MockIdentityService'
    
        # Helper function to create mock cache directories
        function New-MockCache {
            param(
                [string]$CacheName,
                [string[]]$Files = @(),
                [datetime]$CreatedDate = (Get-Date).AddDays(-30),
                [datetime]$LastModified = (Get-Date).AddDays(-1)
            )
        
            $CacheDir = Join-Path $TestCacheRoot $CacheName
            $null = New-Item -Path $CacheDir -ItemType Directory -Force
        
            # Set directory timestamps
            (Get-Item $CacheDir).CreationTime = $CreatedDate
            (Get-Item $CacheDir).LastWriteTime = $LastModified
        
            if ($Files.Count -eq 0) {
                # Create default files - just use some fake cache file names
                $Files = @($CacheName, "$CacheName.lockfile")
            }
        
            foreach ($File in $Files) {
                $FilePath = Join-Path $CacheDir $File
            
                # Create the file if it doesn't exist
                if (-not (Test-Path $FilePath)) {
                    New-Item -Path $FilePath -ItemType File -Force | Out-Null
                }
            
                # Set file timestamps
                (Get-Item $FilePath).CreationTime = $CreatedDate.AddHours(1)
                (Get-Item $FilePath).LastWriteTime = $LastModified.AddHours(1)
            }
        
            return $CacheDir
        }

        # Create mock cache directories for testing
        New-MockCache -CacheName 'TestMSALCache' -CreatedDate (Get-Date).AddDays(-10)
        New-MockCache -CacheName 'TestCustomCache' -Files @('custom.cache') -CreatedDate (Get-Date).AddDays(-5)
        New-MockCache -CacheName 'EmptyCache' -Files @('empty.dat')
        New-MockCache -CacheName 'MultiFileCache' -Files @('msal.cache', 'msal.cache.lockfile', 'additional.log')
    }
    
    Context 'command tests' {
        It 'should return caches' {
            $Caches = Get-AzTokenCache -RootPath $TestCacheRoot
            $Caches | Should -Not -BeNullOrEmpty
            $Caches.Count | Should -BeGreaterThan 0
        }
        It 'should not throw an error with -IncludeAccounts' {
            # Only testing the code path, we won't actually find accounts in the mock caches
            { Get-AzTokenCache -IncludeAccounts -RootPath $TestCacheRoot } | Should -Not -Throw
        }

        It 'Should have required properties when caches exist' {
            $Caches = Get-AzTokenCache -RootPath $TestCacheRoot
            $Cache = $Caches[0]
            $CachePropertyNames = $Cache.PSObject.Properties.Name
            
            $CachePropertyNames | Should -Contain 'Name'
            $CachePropertyNames | Should -Contain 'Path'
            $CachePropertyNames | Should -Contain 'CreatedDate'
            $CachePropertyNames | Should -Contain 'LastModified'
            $CachePropertyNames | Should -Contain 'AccountCount'
            $CachePropertyNames | Should -Contain 'AccountInfoChecked'
            $CachePropertyNames | Should -Contain 'Accounts'
        }
    }
}