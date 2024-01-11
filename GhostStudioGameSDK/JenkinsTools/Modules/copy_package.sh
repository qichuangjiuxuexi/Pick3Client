#!/bin/sh

if [ $# -lt 4 ] ; then
    echo "Usage: $0 projectPath packagePath releaseBuild buildVersion"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

packagePath="$1"
echo "packagePath: $packagePath"
shift

releaseBuild="${1:-false}"
echo "releaseBuild: $releaseBuild"
shift

buildVersion="${1:-1}"
echo "buildVersion: $buildVersion"
shift

#------------------------------检查环境------------------------------
buildPath="${projectPath}/../Build"
if [[ ! -d "${buildPath}" ]]; then
    echo "生成目录不存在: ${buildPath}" >&2
    exit -1
fi

apkPath=$(find "${buildPath}" -maxdepth 1 -type f \( -iname \*.apk -o -iname \*.aab -o -iname \*.ipa \) | head -n 1 | xargs -I {} readlink -f "{}")
if [[ ! -f "${apkPath}" ]]; then
    echo "生成包不存在: ${buildPath}" >&2
    exit -1
fi

settingPath="${projectPath}/ProjectSettings/ProjectSettings.asset"
version=$(grep "bundleVersion:" "$settingPath" | awk -F': ' '{print $2}')
if [[ -z "${version}" ]]; then
    echo "版本号不存在: $settingPath" >&2
    exit -1
fi

extention="${apkPath##*.}"
releaseName=$([[ "${releaseBuild}" == "true" ]] && echo "Release" || echo "Debug")
platform=$( [[ "$extention" == "ipa" ]] && echo "iOS" || echo "Android" )
#使用根文件夹作为包名
appName=$(basename "$(dirname "$projectPath")" | cut -d'_' -f1)
fileName="${appName}_v${version}_${buildVersion}_$(date +%Y%m%d%H%M%S)_${releaseName}"
packagePath="${packagePath}/${appName}--${releaseName}/${platform}"
relativePath="${appName}--${releaseName}/${platform}/${fileName}.${extention}"
echo "${relativePath}" > "${projectPath}/package_path.txt"

#------------------------------拷贝包------------------------------
targetPath="${packagePath}/${fileName}.${extention}"
mkdir -p "${packagePath}"
echo "拷贝包: ${apkPath} -> ${targetPath}"
cp -f "${apkPath}" "${targetPath}"

#------------------------------拷贝符号表------------------------------
zipPath=$(find "${buildPath}" -maxdepth 1 -type f -name "*.symbols.zip" | head -n 1 | xargs -I {} readlink -f "{}")
if [[ -f "${zipPath}" ]]; then
    targetPath="${packagePath}/Symbols/${fileName}.symbols.zip"
    mkdir -p "${packagePath}/Symbols"
    echo "拷贝符号表: ${zipPath} -> ${targetPath}"
    cp -f "${zipPath}" "${targetPath}"
fi

archivePath=$(find "${buildPath}" -maxdepth 1 -type d -name "*.xcarchive" | head -n 1 | xargs -I {} readlink -f "{}")
if [[ -d "${archivePath}" ]] && [[ "${releaseBuild}" == "true" ]]; then
    targetPath="${packagePath}/xcarchive/${fileName}.xcarchive"
    mkdir -p "${packagePath}/xcarchive"
    echo "拷贝符号表: ${archivePath} -> ${targetPath}"
    cp -rf "${archivePath}/" "${targetPath}/"
fi

#------------------------------拷贝items-services------------------------------
plistPath="${buildPath}/items-services.plist"
if [[ "$extention" == "ipa" ]] && [[ -f "$plistPath" ]] && [[ ! -z "$remoteUrl" ]]; then
    plutil -insert items.0.assets.0.url -string "${remoteUrl}/${relativePath}" "${plistPath}"
    htmlPath="${buildPath}/items-services.htm"
    plistUrl="itms-services://?action=download-manifest&url=${fileName}.plist"
    echo "<!DOCTYPE html><html><head><meta http-equiv='refresh' content=\"0; url='${plistUrl}'\" /></head><body><a href='${plistUrl}'>Click to install ${fileName}.${extention}</a></body></html>" > "$htmlPath"
    mkdir -p "${packagePath}/items-services"
    cp -f "${plistPath}" "${packagePath}/items-services/${fileName}.plist"
    cp -f "${htmlPath}" "${packagePath}/items-services/${fileName}.htm"
    echo "拷贝items-services: ${plistPath} -> ${packagePath}/items-services/${fileName}.plist"
fi

#------------------------------拷贝ContentStateData------------------------------
contentStateDataPath="${buildPath}/addressables_content_state.bin"
if [[ -f "${contentStateDataPath}" ]]; then
    targetPath="${packagePath}/ContentStateData/${fileName}.bin"
    mkdir -p "${packagePath}/ContentStateData"
    echo "拷贝ContentStateData: ${contentStateDataPath} -> ${targetPath}"
    cp -f "${contentStateDataPath}" "${targetPath}"
fi