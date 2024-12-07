# PortToNet


## 脚手架模板
1. 可以作为脚手架模板
2. 使用技术Prism.WPF 8.1.97
3. Prism.DryIoc 8.1.97
4. HandyControls 3.5.2
5. 无边框窗体,自带菜单
6. 实现主题切换,语言切换
7. 实现IconFont字体

## 说明
1. iconfont.ttf [Test项目]("https://www.iconfont.cn/manage/index?spm=a313x.manage_type_mylikes.i1.db775f1f3.6b9c3a8153Uals&manage_type=myprojects&projectId=536918")


## Log
1. ComboBox多次切换语言时有异常
2. UDP公网通讯OK
3. 本机IP地址获取OK
4. 语言及主题可保存


## Publish
```cmd
// <project_folder>/Properties/PublishProfiles 文件夹中的 Net8Publish64.pubxml 文件
dotnet publish -p:PublishProfile=Net8Publish64

// 当前目录的AAA.pubxml
dotnet publish -p:PublishProfileFullPath=Net8Publish64.pubxml
```