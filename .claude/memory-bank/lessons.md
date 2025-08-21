# Lessons Learned (Bug Fixes & Gotchas)
**Last Updated**: 2025-08-21

## Husky.NET Sets core.hookspath
**Issue**: Hooks not executing after installation
**Root Cause**: Git needs core.hookspath set to .husky
**Solution**: `dotnet husky install` sets this automatically
**Debug**: `git config --get core.hookspath` should return ".husky"
**Time Wasted**: 35 minutes
**Date**: 2025-08-21

## CI Must Include branch-freshness Dependencies
**Issue**: CI status job failed with missing dependency
**Root Cause**: Forgot to add branch-freshness to needs array
**Solution**: `needs: [build-and-test, code-quality, branch-freshness]`
**Location**: `.github/workflows/ci.yml:278`
**Date**: 2025-08-21

## Chinese Workflow Document Was Wrong Location
**Issue**: Chinese Git workflow guide in wrong folder
**Root Cause**: Placed in 01-Active instead of 03-Reference
**Solution**: Deleted entirely (not needed)
**Lesson**: Check document location matches purpose
**Date**: 2025-08-21

## Build Must Include Godot for Full Validation
**Issue**: C# tests pass but game won't compile
**Root Cause**: Tests only validate pure C# layer
**Solution**: build.ps1 'test' command now builds everything
**Fixed**: 2025-08-15

## GdUnit4 Requires [TestSuite] Attribute
**Issue**: Tests not discovered by runner
**Root Cause**: Missing [TestSuite] on test class
**Solution**: All test classes need [TestSuite] attribute
**Time Wasted**: 45 minutes
**Date**: 2025-08-10