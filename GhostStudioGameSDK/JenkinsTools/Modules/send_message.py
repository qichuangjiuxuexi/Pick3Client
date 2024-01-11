#!/usr/bin/env python3
#coding: utf-8

import sys, traceback
import json, urllib.request, re

def post_message(group, message):
    if "@" in group:
        url = 'http://10.10.31.14:5700/send_discuss_msg'
        data = {"discuss_id": group.replace("@", ""), "message": message}
    elif "#" in group:
        url = 'http://10.10.31.14:5700/send_private_msg'
        data = {"user_id": group.replace("#", ""), "message": message}
    else:
        url = 'http://10.10.31.14:5700/send_group_msg'
        data = {"group_id": group, "message": message}
    req = urllib.request.Request(url, data=json.dumps(data).encode('utf8'))
    req.add_header('Content-Type', 'application/json')
    r = urllib.request.urlopen(req)
    print(json.dumps(data))
    print(r.read())

def post_dingding(token, message):
    url = 'https://oapi.dingtalk.com/robot/send?access_token=' + token
    data = {"msgtype": "text","text": {"content": message}}
    req = urllib.request.Request(url, data=json.dumps(data).encode('utf8'))
    req.add_header('Content-Type', 'application/json')
    r = urllib.request.urlopen(req)
    print(r.read())

def post_feishu(token, message):
    url = 'https://open.feishu.cn/open-apis/bot/v2/hook/' + token
    data = {"msg_type": "text", "content": {"text": message}}
    req = urllib.request.Request(url, data=json.dumps(data).encode('utf8'))
    req.add_header('Content-Type', 'application/json')
    r = urllib.request.urlopen(req)
    print(r.read())

def send_message(group, message):
    groups = group.split("|")
    message = message.replace("|", "\n").replace("\n\n", "\n").strip()
    ids = set()
    for g in groups:
        gid=g.strip()
        if len(gid) == 36:
            post_feishu(gid, message)
        elif len(gid) == 64:
            post_dingding(gid, message)
        else:
            gids=re.findall('^[@#]?\d+', gid)
            if len(gids) > 0 and len(gids[0].strip()) > 0:
                gid = gids[0].strip()
                if gid in ids:
                    continue
                ids.add(gid)
                post_message(gid, message)

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print ("Usage:%s group message"%sys.argv[0])
        exit(1)
    try:
        group = sys.argv[1]
        message = sys.argv[2]
        send_message(group, message)
    except:
        traceback.print_exc()
