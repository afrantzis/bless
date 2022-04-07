src_build_dir = meson.current_build_dir() # Used by tests
assembly_install_dir = join_paths(get_option('libdir'), meson.project_name())

conf_data = configuration_data()
conf_data.set('PACKAGE', meson.project_name())
conf_data.set('VERSION', meson.project_version())
conf_data.set('GETTEXT_PACKAGE', meson.project_name())
conf_data.set('datadir', join_paths(get_option('prefix'), get_option('datadir')))
conf_data.set('blessassemblydir', join_paths(get_option('prefix'), assembly_install_dir))

configure_defines_cs = configure_file(
    input: 'util/ConfigureDefines.cs.tmpl.in',
    output: 'ConfigureDefines.cs',
    configuration: conf_data
)

bless_bin = configure_file(
    input: 'bless-bin.in',
    output: 'bless',
    configuration: conf_data,
    install_dir: join_paths(get_option('prefix'), get_option('bindir')),
)

util_lib = library(
    'Bless.Util',
    files(
        'util/BaseConverter.cs',
        'util/ByteArray.cs',
        'util/Deque.cs',
        'util/FileResourcePath.cs',
        'util/IntervalTree.cs',
        'util/List.cs',
        'util/Portable.cs',
        'util/ProgressCallback.cs',
        'util/RangeCollection.cs',
        'util/Range.cs',
        'util/RedBlackTree.cs',
        'util/TempFile.cs',
        'util/ThreadedAsyncOperation.cs'
    ) + [configure_defines_cs],
    dependencies: glib_sharp_2_dep,
    cs_args: '-r:Mono.Posix',
    install: true,
    install_dir: assembly_install_dir,
)

tools_lib = library(
    'Bless.Tools',
    files(
        'tools/History.cs',
        'tools/Preferences.cs',
        'tools/Session.cs',
    ),
    install: true,
    install_dir: assembly_install_dir,
)

buffers_lib = library(
    'Bless.Buffers',
    files(
        'buffers/ByteBuffer.cs',
        'buffers/ByteBufferAction.cs',
        'buffers/FileBuffer.cs',
        'buffers/IBuffer.cs',
        'buffers/ISaveState.cs',
        'buffers/SaveAsOperation.cs',
        'buffers/SaveInPlaceOperation.cs',
        'buffers/SaveOperation.cs',
        'buffers/Segment.cs',
        'buffers/SegmentCollection.cs',
        'buffers/SimpleBuffer.cs'
    ),
    dependencies: gtk_sharp_2_dep,
    cs_args: '-r:Mono.Posix',
    link_with: [
        tools_lib,
        util_lib
    ],
    install: true,
    install_dir: assembly_install_dir,
)

tools_export_lib = library(
    'Bless.Tools.Export',
    files(
        'tools/export/ExporterOperation.cs',
        'tools/export/IExportBuilder.cs',
        'tools/export/IExporter.cs',
        'tools/export/InterpretedPatternExporter.cs',
        'tools/export/IPatternExporter.cs',
    ),
    cs_args: '-r:Mono.Posix',
    link_with: [
        buffers_lib,
        util_lib
    ],
    install: true,
    install_dir: assembly_install_dir,
)

tools_find_lib = library(
    'Bless.Tools.Find',
    files(
        'tools/find/BMFindStrategy.cs',
        'tools/find/FindAsyncResult.cs',
        'tools/find/FindOperations.cs',
        'tools/find/IFinder.cs',
        'tools/find/IFindStrategy.cs',
    ),
    link_with: [
        buffers_lib,
        util_lib
    ],
    install: true,
    install_dir: assembly_install_dir,
)

gui_drawers_lib = library(
    'Bless.Gui.Drawers',
    files(
        'gui/drawers/AsciiDrawer.cs',
        'gui/drawers/BinaryDrawer.cs',
        'gui/drawers/DecimalDrawer.cs',
        'gui/drawers/Drawer.cs',
        'gui/drawers/HexDrawer.cs',
        'gui/drawers/OctalDrawer.cs',
        'gui/drawers/PixmapManager.cs',
    ),
    dependencies: gtk_sharp_2_dep,
    install: true,
    install_dir: assembly_install_dir,
)

gui_areas_lib = library(
    'Bless.Gui.Areas',
    files(
        'gui/areas/Area.cs',
        'gui/areas/AreaGroup.cs',
        'gui/areas/Highlight.cs',
    ),
    dependencies: gtk_sharp_2_dep,
    link_with: [
        buffers_lib,
        gui_drawers_lib,
        tools_find_lib,
        util_lib
    ],
    install: true,
    install_dir: assembly_install_dir,
)

plugins_lib = library(
    'Bless.Plugins',
    files(
        'plugins/AreaPlugin.cs',
        'plugins/ExportPlugin.cs',
        'plugins/GuiPlugin.cs',
        'plugins/IPluginPreferences.cs',
        'plugins/Plugin.cs',
        'plugins/PluginManager.cs',
    ),
    dependencies: gtk_sharp_2_dep,
    link_with: [
        gui_areas_lib,
        tools_export_lib,
        util_lib
    ],
    install: true,
    install_dir: assembly_install_dir,
)
    
gui_lib = library(
    'Bless.Gui',
    files(
        'gui/DataBook.cs',
        'gui/DataBookFinder.cs',
        'gui/DataViewControl.cs',
        'gui/DataView.cs',
        'gui/DataViewDisplay.cs',
        'gui/dialogs/AboutDialog.cs',
        'gui/dialogs/Alert.cs',
        'gui/dialogs/ErrorAlert.cs',
        'gui/dialogs/ExportDialog.cs',
        'gui/dialogs/FileOverwriteAlert.cs',
        'gui/dialogs/InformationAlert.cs',
        'gui/dialogs/LayoutSelectionDialog.cs',
        'gui/dialogs/PreferencesDialog.cs',
        'gui/dialogs/ProgressDialog.cs',
        'gui/dialogs/RevertConfirmationAlert.cs',
        'gui/dialogs/SaveConfirmationAlert.cs',
        'gui/dialogs/SaveConfirmationMultiAlert.cs',
        'gui/dialogs/WarningAlert.cs',
        'gui/FileChangedBar.cs',
        'gui/FileService.cs',
        'gui/Layout.cs',
        'gui/Services.cs',
        'gui/SessionService.cs',
        'gui/UIService.cs',
        'gui/WidgetGroup.cs',
    ),
    dependencies: gtk_sharp_2_dep,
    cs_args: ['-r:Mono.Posix', '-nowarn:0169'],
    link_with: [
        buffers_lib,
        gui_areas_lib,
        gui_drawers_lib,
        plugins_lib,
        tools_export_lib,
        tools_find_lib,
        tools_lib,
        util_lib,
    ],
    install: true,
    install_dir: assembly_install_dir,
)

gui_plugins_lib = library(
    'Bless.Gui.Plugins',
    files(
        'gui/plugins/BitwiseOperationsPlugin.cs',
        'gui/plugins/ConversionTablePlugin.cs',
        'gui/plugins/CopyOffsetPlugin.cs',
        'gui/plugins/EditOperationsPlugin.cs',
        'gui/plugins/FileExportPlugin.cs',
        'gui/plugins/FileOperationsPlugin.cs',
        'gui/plugins/FindReplacePlugin.cs',
        'gui/plugins/GotoOffsetPlugin.cs',
        'gui/plugins/HelpOperationsPlugin.cs',
        'gui/plugins/HistoryPlugin.cs',
        'gui/plugins/InfobarPlugin.cs',
        'gui/plugins/PatternMatchHighlightPlugin.cs',
        'gui/plugins/ProgressDisplayPlugin.cs',
        'gui/plugins/SelectLayoutPlugin.cs',
        'gui/plugins/SelectRangePlugin.cs',
        'gui/plugins/StatisticsPlugin.cs',
    ),
    dependencies: [gtk_sharp_2_dep, cairo_dep],
    cs_args: ['-r:Mono.Posix', '-nowarn:0169'],
    link_with: [
        buffers_lib,
        gui_areas_lib,
        gui_drawers_lib,
        gui_lib,
        plugins_lib,
        tools_find_lib,
        tools_lib,
        util_lib,
    ],
    install: true,
    install_dir: assembly_install_dir,
)

gui_areas_plugins_lib = library(
    'Bless.Gui.Areas.Plugins',
    files(
        'gui/areas/AsciiArea.cs',
        'gui/areas/BinaryArea.cs',
        'gui/areas/DecimalArea.cs',
        'gui/areas/GroupedArea.cs',
        'gui/areas/HexArea.cs',
        'gui/areas/OctalArea.cs',
        'gui/areas/OffsetArea.cs',
        'gui/areas/SeparatorArea.cs',
    ),
    dependencies: gtk_sharp_2_dep,
    link_with: [
        buffers_lib,
        gui_areas_lib,
        gui_drawers_lib,
        plugins_lib,
        util_lib,
    ],
    install: true,
    install_dir: assembly_install_dir,
)

tools_export_plugins_lib = library(
    'Bless.Tools.Export.Plugins',
    files(
        'tools/export/plugins/HTMLExportBuilder.cs',
        'tools/export/plugins/HTMLExportPlugin.cs',
        'tools/export/plugins/TextExportBuilder.cs',
        'tools/export/plugins/TextExportPlugin.cs',
    ),
    cs_args: ['-r:Mono.Posix'],
    link_with: [
        buffers_lib,
        plugins_lib,
        tools_export_lib,
        util_lib,
    ],
    install: true,
    install_dir: assembly_install_dir,
)

bless = executable(
    'bless',
    files('gui/MainWindow.cs'),
    dependencies: gtk_sharp_2_dep,
    cs_args: ['-r:Mono.Posix', '-nowarn:0169'],
    link_with: [
        buffers_lib,
        gui_areas_lib,
        gui_areas_plugins_lib,
        gui_drawers_lib,
        gui_lib,
        gui_plugins_lib,
        plugins_lib,
        tools_export_lib,
        tools_find_lib,
        tools_lib,
        util_lib,
    ],
    install: true,
    install_dir: assembly_install_dir,
)
