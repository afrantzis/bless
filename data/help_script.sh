#!/bin/sh

base_dir="$1"
docbook_help="$base_dir/index.docbook"
html_help="$base_dir/index.html"

# try to get default browsers from GConf
GCONFTOOL=$(which gconftool-2 2> /dev/null)

if [ -n "$GCONFTOOL" ];
then
    help_browser=$("$GCONFTOOL" --get "/desktop/gnome/url-handlers/ghelp/command")
    help_browser=$(echo "$help_browser" | sed s/\"//g)
    http_browser=$("$GCONFTOOL" --get "/desktop/gnome/url-handlers/http/command")
    http_browser=$(echo "$http_browser" | sed s/\"//g)
fi

# some other browsers
yelp_browser=$(which yelp 2> /dev/null)
firefox_browser=$(which firefox 2> /dev/null)

([ -n "$help_browser" ] && $(printf "$help_browser" "$docbook_help")) ||
([ -n "$yelp_browser" ] && "$yelp_browser" "$docbook_help") ||
([ -n "$http_browser" ] && $(printf "$http_browser" "$html_help")) ||
([ -n "$firefox_browser" ] && "$firefox_browser" "$html_help")
