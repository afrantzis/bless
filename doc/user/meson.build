gnome = import('gnome')

help_sources = [
    'index.docbook',
    'legal.xml',
]

help_html = [
    'ch01.html',
    'ch02.html',
    'ch02s02.html',
    'ch03.html',
    'ch03s02.html',
    'ch04.html',
    'ch04s02.html',
    'ch04s03.html',
    'ch04s04.html',
    'ch04s05.html',
    'ch04s06.html',
    'ch05.html',
    'ch05s02.html',
    'ch05s03.html',
    'ch05s04.html',
    'ch06.html',
    'ch07.html',
    'index.html',
]

help_media = [
    'figures/bless_area_focus.png',
    'figures/bless_bitwise_ops.png',
    'figures/bless_display_tag.png',
    'figures/bless_edit_mode.png',
    'figures/bless_export_dialog.png',
    'figures/bless_goto.png',
    'figures/bless_layout_example1.png',
    'figures/bless_layout_oha.png',
    'figures/bless_number_base.png',
    'figures/bless_preferences_general.png',
    'figures/bless_preferences_session.png',
    'figures/bless_preferences_undo.png',
    'figures/bless_search_find.png',
    'figures/bless_search_replace.png',
    'figures/bless_select_range.png',
    'figures/bless_start_window.png',
    'figures/bless_tab_close.png',
]

if get_option('html_user_doc')
    html_xsl = 'http://docbook.sourceforge.net/release/xsl/current/html/chunk.xsl'
    xsltproc = find_program('xsltproc', required: false)

    if xsltproc.found()
        cmd = run_command([xsltproc, '--nonet', html_xsl])
        if cmd.returncode() != 0
            error('No XSLT stylesheet found, cannot generating html documentation. You can disable with -Dhtml_user_doc=false')
        endif
    else
        error('No XSLT processor found, cannot generating html documentation. You can disable with -Dhtml_user_doc=false')
    endif

    custom_target(
        'help-html',
        command: [xsltproc, '--nonet', '-o', '@OUTDIR@/', html_xsl, '@INPUT@'],
        input: files('C/index.docbook'),
        output: help_html,
        build_by_default: true,
        install: true,
        install_dir: join_paths(get_option('datadir'), 'help', 'C', meson.project_name()),
    )
endif

gnome.yelp(            
    meson.project_name(),       
    sources: help_sources,
    media: help_media,
    symlink_media: true,
)    
