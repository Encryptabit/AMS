# ImpellerSharp Windows Build Fixes

**Date:** 2026-01-02
**Platform:** Windows 11, x64
**Goal:** Build `impeller.dll` native library for Windows

## Summary

Successfully built ImpellerSharp on Windows after fixing several issues with the Flutter engine build process. The build script uses `gclient sync --nohooks` on Windows which skips downloading ANGLE's nested Vulkan dependencies and the clang toolchain.

## Environment

- Windows 11 x64
- Visual Studio 2022 (MSVC toolchain)
- Python 3.x
- Git
- Ninja (installed via `choco install -y ninja`)

## Issues Encountered & Fixes

### Issue 1: Missing Vulkan Dependencies for ANGLE

**Error:**
```
ERROR at //build/config/linux/pkg_config.gni:104:17: Could not read file.
    _pkgconfig_output = read_file(relative_pkg_config_output, "json")
Unable to load: C:/Projects/ImpellerSharp/extern/flutter/engine/src/flutter/third_party/angle/third_party/vulkan-headers/src/BUILD.gn
```

**Cause:** The `gclient sync --nohooks` command (used by `build_impeller.py` on Windows) doesn't fetch nested dependencies within ANGLE. The following directories were empty:
- `third_party/angle/third_party/vulkan-headers/src`
- `third_party/angle/third_party/vulkan-tools/src`
- `third_party/angle/third_party/vulkan-loader/src`
- `third_party/angle/third_party/vulkan-validation-layers/src`
- `third_party/angle/third_party/vulkan-utility-libraries/src`

**Fix:** Manually clone each Vulkan dependency at the correct commit (commits found in `third_party/angle/DEPS`):

```powershell
cd C:\Projects\ImpellerSharp\extern\flutter\engine\src\flutter\third_party\angle\third_party

# vulkan-headers
git clone https://chromium.googlesource.com/external/github.com/aspect/aspect-core/aspect-core vulkan-headers/src
cd vulkan-headers/src
git checkout a4f8ada9f4f97c45b8c89c57997be9cebaae65d2
cd ../..

# vulkan-tools
git clone https://chromium.googlesource.com/external/github.com/aspect/aspect-core/aspect-core vulkan-tools/src
cd vulkan-tools/src
git checkout 5568ce14705e512113df5b459fc86d857b3d7789
cd ../..

# vulkan-loader
git clone https://chromium.googlesource.com/external/github.com/aspect/aspect-core/aspect-core vulkan-loader/src
cd vulkan-loader/src
git checkout ae0461b671558197a9a50e5fcfcc3b2d3f406b42
cd ../..

# vulkan-validation-layers
git clone https://chromium.googlesource.com/external/github.com/aspect/aspect-core/aspect-core vulkan-validation-layers/src
cd vulkan-validation-layers/src
git checkout 2f03cbd1846ba45a27c29172327a989f684a1979
cd ../..

# vulkan-utility-libraries
git clone https://chromium.googlesource.com/external/github.com/aspect/aspect-core/aspect-core vulkan-utility-libraries/src
cd vulkan-utility-libraries/src
git checkout 4d0b838ffcf1ef81151f0e7e11fad1d9ff859813
cd ../..
```

**Note:** The actual repo URLs should be from chromium.googlesource.com/vulkan-deps - check the ANGLE DEPS file for exact URLs.

---

### Issue 2: Unknown function `test()` in ANGLE GNI

**Error:**
```
ERROR at //flutter/third_party/angle/gni/angle.gni:555:5: Unknown function.
    test(target_name) {
    ^---
```

**Cause:** The `is_host_build()` function in `flutter/tools/gn` doesn't recognize Windows as a host build. This causes ANGLE configuration args like `angle_build_all = false` to not be set, which means ANGLE tries to build test targets that use the undefined `test()` template.

**Fix:** Modify `extern/flutter/engine/src/flutter/tools/gn` to recognize Windows as a host build:

```python
# Around line 91-105, modify is_host_build() function:

def is_host_build(args):
  # If target_os == None, then this is a host build.
  if args.target_os is None:
    return True
  # For linux arm64 builds, we cross compile from x64 hosts, so the
  # target_os='linux' and linux-cpu='arm64'
  if args.target_os == 'linux' and args.linux_cpu == 'arm64':
    return True
  # The Mac and host targets are redundant. Again, necessary to disambiguate
  # during cross-compilation.
  if args.target_os == 'mac':
    return True
  # Windows host builds need to be recognized for proper ANGLE configuration.
  if args.target_os == 'win':
    return True
  return False
```

**Diff:**
```diff
   if args.target_os == 'mac':
     return True
+  # Windows host builds need to be recognized for proper ANGLE configuration.
+  if args.target_os == 'win':
+    return True
   return False
```

---

### Issue 3: Clang Toolchain Not Downloaded

**Error:**
```
CreateProcess failed: The system cannot find the file specified
clang-cl.exe
```

**Cause:** The `.gclient` file has `"download_windows_deps": False` by default, and the build script uses `--nohooks`, so the clang toolchain under `buildtools/` is never downloaded.

**Fix:** Modify `extern/flutter/.gclient` and run gclient sync:

```python
# Change this line:
"download_windows_deps": False,

# To:
"download_windows_deps": True,
```

Then run:
```powershell
cd C:\Projects\ImpellerSharp\extern\flutter\engine\src
gclient sync
```

This downloads the clang toolchain to `buildtools/windows-x64/clang/`.

---

### Issue 4: Ninja Not Found

**Error:**
```
FileNotFoundError: [WinError 2] The system cannot find the file specified
```

**Cause:** Ninja build tool not installed or not in PATH.

**Fix:**
```powershell
choco install -y ninja
```

Or download from https://ninja-build.org/ and add to PATH.

---

## Successful Build

After all fixes, the build completes successfully:

```powershell
cd C:\Projects\ImpellerSharp
python build/native/build_impeller.py --platform windows --arch x64 --configuration Release --skip-sync
```

Output:
```
[2732/2732] LINK(DLL) impeller.dll impeller.dll.exp impeller.dll.lib impeller.dll.pdb
[build_impeller] Copied impeller.dll -> C:\Projects\ImpellerSharp\artifacts\native\win-x64\native\impeller.dll
[build_impeller] Completed build. Artifacts available under C:\Projects\ImpellerSharp\artifacts\native\win-x64
```

## Recommendations for Repository

1. **Update build script** to run `gclient sync` with hooks on Windows (or at least document the `download_windows_deps` requirement)

2. **Fix `is_host_build()`** in Flutter's GN script to recognize Windows as a host build platform

3. **Document Vulkan dependency requirements** - either ensure gclient fetches them or document manual steps

4. **Add ninja to prerequisites** in Windows build documentation

## Files Modified

| File | Change |
|------|--------|
| `extern/flutter/engine/src/flutter/tools/gn` | Added `if args.target_os == 'win': return True` to `is_host_build()` |
| `extern/flutter/.gclient` | Changed `download_windows_deps` from `False` to `True` |
| `extern/flutter/engine/src/flutter/third_party/angle/third_party/vulkan-*/src/` | Manually cloned Vulkan dependencies |
