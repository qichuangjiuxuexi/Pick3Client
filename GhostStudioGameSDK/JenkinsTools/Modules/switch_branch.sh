#!/bin/sh

if [ $# -lt 2 ] ; then
    echo "Usage: $0 projectPath branchName [merge_branch]"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

branchName="${1:-main}"
echo "branchName: $branchName"
shift

mergeBranch="$1"
echo "mergeBranch: $mergeBranch"
shift

#------------------------------检查环境------------------------------
if [[ ! -d "${projectPath}" ]]; then
    echo "Unity工程不存在: ${projectPath}" >&2
    exit -1
fi

cd "$projectPath"
gitPath=$(git rev-parse --show-toplevel)
if [[ ! -d "${gitPath}" ]]; then
    echo "Git工程不存在: ${gitPath}" >&2
    exit -1
fi
cd "$gitPath"

#------------------------------保存更改记录------------------------------
lastBranch=$(git rev-parse --abbrev-ref HEAD)
if [ "$lastBranch" == "$branchName" ]; then
    lastCommit=$(git rev-parse --short HEAD)
fi

#------------------------------切换分支------------------------------
rm -f .git/index.lock
git fetch
git checkout -f "$branchName" || exit $?
git reset --hard "origin/$branchName"
git clean -fd
git submodule update || exit $?
git reset --hard HEAD

#------------------------------合并分支------------------------------
if [[ ! -z "$mergeBranch" ]]; then
    git merge "origin/$mergeBranch" -m AutoMerge || exit $?
fi

#------------------------------写入更改记录------------------------------
historyPath="${projectPath}/git_history.txt"
# if [[ "$lastBranch" == "$branchName" ]]; then
    git log --oneline --no-merges --pretty=format:"%s" ${lastCommit}.. > "$historyPath"
    echo >> "$historyPath"
# else
#     echo "切换分支到 $branchName" > "$historyPath"
# fi
