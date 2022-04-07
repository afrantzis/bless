project(
    'bless',
    'cs',
    version: '0.6.3',
    meson_version: '>= 0.46',
    default_options: ['libdir=lib'],
)

add_project_arguments('-d:ENABLE_UNIX_SPECIFIC', language: 'cs')

gtk_sharp_2_dep = dependency('gtk-sharp-2.0')
glib_sharp_2_dep = dependency('glib-sharp-2.0')
cairo_dep = dependency('mono-cairo')

subdir('src')
subdir('data')
if get_option('tests')
    subdir('tests')
endif
if get_option('user_doc')
    subdir('doc/user')
endif
