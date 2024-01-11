#!/bin/sh
if [ $# -lt 2 ] ; then
    echo "Usage: $0 uploadKeys deleteKeys"
    exit -1
fi

uploadKeys="${1}"
shift

deleteKeys="${1}"
shift

#------------------------------检查环境------------------------------
currentShellPath=$(cd "$(dirname "$0")"; pwd)
jsonPath="$currentShellPath/../../../Cer/firebase.json"
if [[ ! -f "${jsonPath}" ]]; then
    echo "Firebase配置文件不存在: ${jsonPath}" >&2
    exit -1
fi

exportsPath="$currentShellPath/../../../Exports/Configs"
if [[ ! -d "${exportsPath}" ]]; then
    echo "生成目录不存在: ${exportsPath}" >&2
    exit -1
fi

#------------------------------上传配置到firebase------------------------------
cd "$currentShellPath"
npm install || exit $?
node firebase_upload_config.js "$uploadKeys" "$deleteKeys" || exit $?
