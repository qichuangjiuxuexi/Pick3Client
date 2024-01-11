const admin = require('firebase-admin');
const fs = require('fs');
const path = require('path');
const serviceAccount = require('../../../Cer/firebase.json');

async function updateRemoteConfig(uploadKeys, deleteKeys) {
  try {
    //登录
    console.log('Firebase logining...');
    admin.initializeApp({
      credential: admin.credential.cert(serviceAccount)
    });

    //获取模板
    const remoteConfig = admin.remoteConfig();
    console.log('Firebase getting template...');
    const template = await remoteConfig.getTemplate();
    console.log(`Firebase current template version: ${JSON.stringify(template.version)}`);
    const oldKeys = Object.keys(template.parameters);
    console.log(`Firebase current template keys: ${oldKeys}`);
    
    //更新模板
    const dir = '../../../Exports/Configs';
    const files = fs.readdirSync(dir);
    uploadKeys.forEach(key => {
      //查找忽略大小写的配置名
      const match = files.find(f => f.toLowerCase() === `${key.toLowerCase()}.json`);
      if (!match) {
        // 如果没有找到匹配的文件，则退出程序
        console.error(`File not found for key: ${key}`);
        process.exit(1);
      }
      const newKey = path.basename(match, '.json');
      console.log(`Firebase add key: ${newKey}`);
      template.parameters[newKey] = {
        defaultValue: {
          value: fs.readFileSync(`../../../Exports/Configs/${match}`, 'UTF8')
        },
        valueType: 'JSON'
      };
    });

    //删除模板
    deleteKeys.forEach(key => {
      const delKey = oldKeys.find(k => k.toLowerCase() === key.toLowerCase())
      if (delKey) {
        console.log(`Firebase del key: ${delKey}`);
        delete template.parameters[delKey];
      }
    })

    //验证模板
    console.log('Firebase validating template...');
    const validation = await remoteConfig.validateTemplate(template);
    console.log(`Firebase new template keys: ${Object.keys(validation.parameters)}`);
    fs.writeFileSync("firebase_upload_config.log", Object.keys(validation.parameters).join(', '), 'UTF8');

    //上传模板
    console.log(`Firebase uploading template...`);
    await remoteConfig.publishTemplate(validation);
    console.log(`Firebase upload success`);
  } catch (error) {
    console.error('Firebase error:', error);
    process.exit(1);
  }
}

//读取参数
const uploadKeys = process.argv[2] || "";
const deleteKeys = process.argv[3] || "";
if (!uploadKeys && !deleteKeys) {
  console.error('Usage: node firebase_push_remote_config.js uploadKeys deleteKeys');
  process.exit(1);
}

updateRemoteConfig(uploadKeys.split(',').map(x=>x.trim()), deleteKeys.split(',').map(x=>x.trim()));
