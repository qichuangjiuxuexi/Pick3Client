#!/bin/sh

if [ $# -lt 3 ] ; then
    echo "Usage: $0 projectPath message folders"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

message="$1"
echo "message: $message"
shift

#------------------------------检查环境------------------------------
if [[ ! -d "${projectPath}" ]]; then
    echo "Unity工程不存在: ${projectPath}" >&2
    exit -1
fi
cd "$projectPath"

#------------------------------提交git------------------------------
git pull --rebase --autostash || exit $?
git add "$@" || exit $?
git commit -m "$message" || exit $?
git push || exit $?
