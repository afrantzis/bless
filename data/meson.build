install_subdir(
    'ui',
    install_dir: join_paths(get_option('datadir'), meson.project_name()),
)

install_data(
    files(
        'bless-16-bytes-per-row.layout',
        'bless-48x48.png',
        'bless-about.png',
        'bless-all-number-bases.layout',
        'bless-binary-ascii.layout',
        'bless-courier.ttf',
        'bless-default.layout',
        'default-preferences.xml',
        'help_script.sh',
    )
)

install_data(
    'bless.desktop',
    install_dir: join_paths(get_option('datadir'), 'applications'),
)

install_data(
    'bless-48x48.png',
    rename: 'bless.png',
    install_dir: join_paths(get_option('datadir'), 'icons', 'hicolor', '48x48', 'apps'),
)
