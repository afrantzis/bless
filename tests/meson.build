nunit_console = find_program('nunit-console', required: false)
nunit_dep = dependency('nunit', required: false)

if not nunit_dep.found()
    error('nunit not found, cannot build tests. You can disable with -Dtests=false')
endif

test_env = environment()
test_env.append('MONO_PATH', src_build_dir)

tests_buffers_lib = library(
    'BlessTests.Buffers',
    files(
        'buffers/ByteBufferTests.cs',
        'buffers/FileBufferTests.cs',
        'buffers/SegmentCollectionTests.cs',
        'buffers/SegmentTests.cs',
        'buffers/SimpleBufferTests.cs',
    ),
    dependencies: nunit_dep,
    link_with: [
        buffers_lib,
        util_lib,
    ],
)

# The tests expect the test data files to be in the directory the runner runs in.
# nunit-console changes directory to the module under test before running,
# so copy the data files to that same build directory.
meson.add_postconf_script(
    'copy_test_data.py',
    join_paths(meson.current_source_dir(), 'buffers', 'test1.bin'),
    join_paths(meson.current_build_dir(), 'test1.bin'))

if get_option('run_tests')
    if not nunit_console.found()
        warning('nunit-console not found, cannot run tests. You can disable with -Drun_tests=false')
        subdir_done()
    endif

    test(
        'Buffers',
        nunit_console,
        args: tests_buffers_lib,
        env: test_env,
    )

    tests_tools_export_lib = library(
        'BlessTests.Tools.Export',
        files(
            'tools/export/ExporterTests.cs',
        ),
        dependencies: nunit_dep,
        link_with: [
            buffers_lib,
            tools_export_lib,
            tools_export_plugins_lib,
        ],
    )

    test(
        'Tools.Export',
        nunit_console,
        args: tests_tools_export_lib,
        env: test_env,
    )

    tests_util_lib = library(
        'BlessTests.Util',
        files(
            'util/ByteArrayTests.cs',
            'util/DequeTests.cs',
            'util/IntervalTreeTests.cs',
            'util/ListTests.cs',
            'util/RangeTests.cs',
            'util/RedBlackTreeTests.cs',
        ),
        dependencies: nunit_dep,
        link_with: [
            util_lib,
        ],
    )

    test(
        'Util',
        nunit_console,
        args: tests_util_lib,
        env: test_env,
    )
endif
