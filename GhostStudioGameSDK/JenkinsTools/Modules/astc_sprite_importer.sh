#!/bin/sh

if [ $# -lt 2 ] ; then
    echo "Usage: $0 projectPath platform"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

platform="${1:-Android}"
echo "platform: $platform"
shift

#------------------------------检查环境------------------------------
currentShellPath=$(cd "$(dirname "$0")"; pwd)
unityVersion=$(ls /Applications/Unity/Hub/Editor/ | sort -r | head -n 1)
MONO_PATH="/Applications/Unity/Hub/Editor/${unityVersion}/Unity.app/Contents/MonoBleedingEdge/bin/mono"
if [[ ! -f "${MONO_PATH}" ]]; then
    echo "Unity 程序不存在: ${MONO_PATH}" >&2
    exit -1
fi

if [[ ! -d "${projectPath}" ]]; then
    echo "Unity 工程不存在: ${projectPath}" >&2
    exit -1
fi

[[ "${platform}" == "iOS" ]] && platform="iPhone"

#------------------------------压缩ASTC------------------------------
astcImporterPath="${currentShellPath}/bin/AstcSpriteImporter.exe"
echo "$MONO_PATH" "$astcImporterPath" $platform "${projectPath}/Assets"
"$MONO_PATH" "$astcImporterPath" $platform "${projectPath}/Assets"
