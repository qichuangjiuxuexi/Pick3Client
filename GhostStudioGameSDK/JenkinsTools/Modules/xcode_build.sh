#!/bin/sh

if [ $# -lt 2 ] ; then
    echo "Usage: $0 projectPath releaseBuild"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

releaseBuild="${1:-false}"
echo "releaseBuild: $releaseBuild"
shift

echo "------------------------------检查环境------------------------------"
currentShellPath=$(cd "$(dirname "$0")"; pwd)
xcodeProjectPath="${projectPath}/../Build/XcodeProject"
if [[ ! -d "${xcodeProjectPath}" ]]; then
    echo "Xcode工程不存在: ${xcodeProjectPath}" >&2
    exit -1
fi

[[ -d "${xcodeProjectPath}/Unity-iPhone.xcworkspace" ]] && projPath="-workspace Unity-iPhone.xcworkspace" || projPath="-project Unity-iPhone.xcodeproj"

echo "------------------------------准备工作------------------------------"
xcarchivePath="${projectPath}/../Build/Unity-iPhone.xcarchive"
rm -rf "${xcarchivePath}"

cd "${xcodeProjectPath}"
xcodebuild clean

if [[ ! -z "$keychainPassword" ]]; then
    keychainPath="$HOME/Library/Keychains/Login.keychain"
    security unlock-keychain -p "$keychainPassword" "$keychainPath"
    security -v list-keychains -s "$keychainPath"
    security -v set-keychain-settings -t 3600 -l "$keychainPath"
    security -v default-keychain -s "$keychainPath"
fi

cpu_count=`sysctl -n hw.physicalcpu`
echo "cpu_count=$cpu_count"
defaults write com.apple.dt.xcodebuild PBXNumberOfParallelBuildSubtasks $cpu_count
defaults write com.apple.dt.xcodebuild IDEBuildOperationMaxNumberOfConcurrentCompileTasks $cpu_count
defaults write com.apple.dt.Xcode PBXNumberOfParallelBuildSubtasks $cpu_count
defaults write com.apple.dt.Xcode IDEBuildOperationMaxNumberOfConcurrentCompileTasks $cpu_count

echo "------------------------------Xcode构建Arvhie------------------------------"
echo "
xcodebuild \
    -sdk iphoneos \
    -configuration Release \
    ${projPath} \
    -scheme Unity-iPhone \
    -archivePath ${xcarchivePath} \
    -allowProvisioningUpdates \
    archive"
xcodebuild \
    -sdk iphoneos \
    -configuration Release \
    ${projPath} \
    -scheme Unity-iPhone \
    -archivePath "${xcarchivePath}" \
    -allowProvisioningUpdates \
    archive
if [[ ! -d "${xcarchivePath}" ]]; then
    echo "Archive 生成失败: ${xcarchivePath}" >&2
    exit -1
fi

echo "------------------------------生成Plist配置文件------------------------------"
buildPath="${projectPath}/../Build"
plistPath="${buildPath}/export_archive.plist"
echo '<?xml version="1.0" encoding="UTF-8"?><!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd"><plist version="1.0"><dict/></plist>' > "${plistPath}"
plutil -insert destination -string "export" "${plistPath}"
plutil -insert stripSwiftSymbols -bool true "${plistPath}"
if [ "$releaseBuild" == "true" ]; then
    plutil -insert method -string "ad-hoc" "${plistPath}"
    plutil -insert compileBitcode -bool false "${plistPath}"
    plutil -insert thinning -string "<none>" "${plistPath}"
else
    plutil -insert method -string "development" "${plistPath}"
    plutil -insert compileBitcode -bool false "${plistPath}"
    plutil -insert thinning -string "<none>" "${plistPath}"
fi

echo "------------------------------生成ipa包------------------------------"
echo "xcodebuild \
    -exportArchive \
    -allowProvisioningUpdates \
    -archivePath ${xcarchivePath} \
    -exportOptionsPlist ${plistPath} \
    -exportPath ${buildPath}"
xcodebuild \
    -exportArchive \
    -allowProvisioningUpdates \
    -archivePath "${xcarchivePath}" \
    -exportOptionsPlist "${plistPath}" \
    -exportPath "${buildPath}"

if ! ls "${buildPath}"/*.ipa; then
    echo "ipa包生成失败: ${buildPath}" >&2
    exit -1
fi

#------------------------------生成items-services------------------------------
buildSettings=$(xcodebuild -showBuildSettings -scheme Unity-iPhone ${projPath})
bundleIdentifier=$(echo "$buildSettings" | grep PRODUCT_BUNDLE_IDENTIFIER | awk -F' = ' '{print $2}')
appName=$(echo "$buildSettings" | grep "^\\s*PRODUCT_NAME =" | awk -F' = ' '{print $2}')
bundleVersion=$(echo "$buildSettings" | grep MARKETING_VERSION | awk -F' = ' '{print $2}')
plistPath="${buildPath}/items-services.plist"
echo '<?xml version="1.0" encoding="UTF-8"?><!DOCTYPE plist PUBLIC "-//Apple Computer//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd"><plist version="1.0"><dict><key>items</key><array><dict><key>assets</key><array><dict><key>kind</key><string>software-package</string></dict></array><key>metadata</key><dict><key>kind</key><string>software</string></dict></dict></array></dict></plist>' > ${plistPath}
plutil -insert items.0.metadata.title -string "${appName}" "${plistPath}"
plutil -insert items.0.metadata.bundle-identifier -string "${bundleIdentifier}" "${plistPath}"
plutil -insert items.0.metadata.bundle-version -string "$bundleVersion" "${plistPath}"
