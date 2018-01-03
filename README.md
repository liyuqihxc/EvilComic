# 邪恶漫画

自动下载某站的不可描述的漫画。

项目使用 ASP.NET Core MVC + EF Core 2.0 + Sqlite 可以部署在Windows或任何 .Net Core 2.0 支持的系统上。


## 注意

*   作者无意传播任何非法内容，程序当中也没有任何图片、视频、文字为非法内容。
*   使用者需自行承担使用此软件带来的一切后果。

## How to

*   复制 DBTemplate 目录下的 XEDbIndex.sqlite 文件到任意目录，修改 appsettings.json 文件当中的连接字符串。
*   在 XEDbIndex 数据库的 XEDbPath 表中添加数据文件的存放目录，保证目录所在驱动器的剩余空间大于 2GB。
*   按照 ASP.NET Core 网站部署的方法来部署本程序。


