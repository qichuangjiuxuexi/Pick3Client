#!/bin/sh

if [ $# -lt 3 ] ; then
    echo "Usage: $0 projectPath buildTarget methodName params"
    exit -1
fi
echo $0 $@

projectPath="$1"
echo "projectPath: $projectPath"
shift

buildTarget="${1:-Android}"
echo "buildTarget: $buildTarget"
shift

methodName="$1"
echo "methodName: $methodName"
shift

echo "params: $@"

#------------------------------检查环境------------------------------
currentShellPath=$(cd "$(dirname "$0")"; pwd)
if [[ ! -d "${projectPath}" ]]; then
    echo "Unity工程不存在: ${projectPath}" >&2
    exit -1
fi

unityVersion=$(grep "m_EditorVersion:" "${projectPath}/ProjectSettings/ProjectVersion.txt" | awk -F': ' '{print $2}')
UNITY_PATH="/Applications/Unity/Hub/Editor/${unityVersion}/Unity.app/Contents/MacOS/Unity"
if [[ ! -f "${UNITY_PATH}" ]]; then
    echo "Unity程序不存在: ${UNITY_PATH} " >&2
    exit -1
fi

cd "$projectPath"
if [[ -f editor.pid ]]; then
    /bin/echo -n "--- 杀死上次残留的Unity进程: "
    cat editor.pid
    pkill -F editor.pid
fi

echo "------------------------------Unity调用------------------------------"
echo "$UNITY_PATH -quit -batchmode -logfile editor.txt -buildTarget $buildTarget -projectPath $projectPath -executeMethod $methodName $@"
"$UNITY_PATH" -quit -batchmode -logfile editor.txt -buildTarget $buildTarget -projectPath "$projectPath" -executeMethod "$methodName" "$@" & pid=$!
echo $pid > editor.pid
echo "Unity PID is: $pid"
sleep 1
"$currentShellPath/bin/gtail" --pid=$pid -f editor.txt &
wait $pid
exit_code=$?
sleep 1
pkill -F editor.pid
rm -f editor.pid
exit $exit_code
