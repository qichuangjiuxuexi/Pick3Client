#!/bin/sh

if [ $# -lt 5 ] ; then
    echo "Usage: $0 qqGroup qqMsg projectPath branchName remoteUrl"
    exit -1
fi
echo $0 $@

qqGroup="$1"
echo "qqGroup: $qqGroup"
shift

qqMsg="$1"
echo "qqMsg: $qqMsg"
shift

projectPath="$1"
echo "projectPath: $projectPath"
shift

branchName="${1:-main}"
echo "branchName: $branchName"
shift

remoteUrl="$1"
echo "remoteUrl: $remoteUrl"
shift

#------------------------------检查环境------------------------------
currentShellPath=$(cd "$(dirname "$0")"; pwd)
packagePath=$(cat "${projectPath}/package_path.txt")
if [[ -z "${packagePath}" ]]; then
    echo "包路径不存在: ${projectPath}/package_path.txt" >&2
    exit -1
fi

extention="${packagePath##*.}"
[[ "${extention}" == "ipa" ]] && icon="🍎" || icon="📦"
[[ "${extention}" == "ipa" ]] && platform="iOS" || platform="Android"
packageUrl="${remoteUrl}/${packagePath}"
#使用根文件夹作为包名
appName=$(basename "$(dirname "$projectPath")" | cut -d'_' -f1)
buildVersion=$(echo "$packagePath" | awk -F'_' '{print $(NF-2)}')
version=$(echo "$packagePath" | awk -F'_' '{print $(NF-3)}')
fileName="${packagePath##*/}"
packageDir=$(dirname "$packagePath")
[[ "${extention}" == "ipa" ]] && installMsg="|安装地址：${remoteUrl}/${packageDir}/items-services/${fileName%.*}.htm"

#------------------------------生成git日志------------------------------
historyPath="$projectPath/git_history.txt"
if [[ -z "$qqMsg" ]] && [[ -f "$historyPath" ]]; then
    while read -r line; do
        echo $line
        qqMsg="$qqMsg|　　$line"
    done < "$historyPath"
fi

#------------------------------发送消息------------------------------
qqMsg="${icon} ${appName} ${platform}打包完毕：编号：${buildVersion}|打包分支：${branchName}|下载地址：${packageUrl}${installMsg}|修改信息：${qqMsg}"
if [[ -z "$qqGroup" ]] || [[ "$qqGroup" == "0" ]]; then
    echo "qqGroup ${qqGroup}为空，忽略发送消息: $qqMsg"
else
    "$currentShellPath/send_message.py" "$qqGroup" "$qqMsg"
fi
