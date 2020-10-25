此项目是为面试准备，包括slg大地图简易实现及一些基础框架，整体比较简单，只是方便给面试官看下我的编码及一些平时积累文档，方便面试官了解。

因为自己平时写的一些框架或小demo都放在私有库中，这次为了面试整理了一些自己写的前端部分代码，但是整理之后感觉东西有点少，由于公司项目的slg地图是分块式的，并不是像传统slg一样的超大地图形式，所以正好趁此机会，也为了demo更丰富一些，花了一些业余时间写了一个slg大地图demo，由于时间比较紧 ，所以只是一个简易版本。

## 使用
* 此项目基于Unity 2019.4.0f1
* 打开Assets/Demo场景，运行即可
* 如果要使用ab，需勾选Main Camera中GameStart脚本的Use Asset Bundle，然后在工具栏中选择Tools/AssetBundleWindow,在打开的窗口中点击开始执行即可

## 大地图部分(Script/SlgWorld)：
完全从0开发的，整体分为几个模块：数据管理、坐标管理、缓存管理、逻辑分了多个handler解耦
* 地图块是菱形形式，共一百万格，并且使用了GPU Instancing来优化
* 通过当前摄像机范围，动态显示范围内的地图块及模型，由于时间有限，地图atlas和模型是网上找的，并且显示的块是随机的(WorldTileHandler、WorldCameraHandler)
* 和模拟服务器通信使用了AOI，目前使用的是比较简易的灯塔形式(AOICoordinates、 WorldAOIChangeHandler、WorldServerSimulateHandler)
* 将数据存放至多个byte数组中，通过对菱形坐标的线性变换映射数组下标，内存和效率都是友好的(WorldDataManager)
* 缓存池基本思想是设定屏幕外的缓存上限，超过上限则卸载最先加入的物体，为了降低时间复杂度，使用了字典+双向链表的数据结构，使得插入、获取、移除都是O(1)的时间复杂度(WorldModelPool、WorldObjectHandler)

## 框架部分(Script/Core)
资源加载相关(AssetLoad)，以及一键打包热更等框架(Editor/AssetBundle）:
* Editor窗口：Tools/AssetBundleWindow
* 打包代码会指定一级资源（目前指定的是项目中所有的prefab及场景），并根据一级资源自动最优粒度划分依赖进行打包ab
* 热更新会自动寻找hash差异并压缩成更新包上传
* 集成了基于7z的压缩

多语言框架(Language)及对应工具(Editor/Language、根目录/LanguageExcelCreator(WinForm做的小程序，给策划生成excel多语言用的))
* Editor窗口：Tools/LanguageWindow
* 可一键导出excel及一键导入项目，编辑和使用都很方便
* 代码：文字需要写在单独类中，生成excel时将变量作为key，读取时通过key及当前选择的语言，反射回类中变量
* prefab和数据表：开发时无需特殊处理，生成excel时查找所有带有中文的，并根据当前prefab名或表名生成唯一key自动填入excel中

网络基本框架(Network)
* 使用的比较底层的api和原始的byte编码解码

UI基本框架(UI)
数据框架(Data)
另外还有一些事件管理、时间管理、声音管理等


