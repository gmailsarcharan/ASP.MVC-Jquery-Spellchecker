$(function () {
    $('textarea.tinymce').tinymce({
        // Location of TinyMCE script
        script_url: tinyMCELocation,

        // General options
        theme: "advanced",
        plugins: "jqueryspellchecker",

        // Theme options
        theme_advanced_buttons1: "bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,formatselect,fontselect,fontsizeselect,|,jqueryspellchecker",
        theme_advanced_buttons2: "cut,copy,paste,pastetext,pasteword,|,search,replace,|,bullist,numlist,|,outdent,indent,blockquote,|,undo,redo,|,link,unlink,cleanup,|,forecolor,backcolor,|,hr,removeformat,visualaid,|,sub,sup",
        theme_advanced_toolbar_location: "top",
        theme_advanced_toolbar_align: "left",
        theme_advanced_statusbar_location: "bottom",
        theme_advanced_resizing: true,
        //location of the spell check css.  this will show red text and underlines.
        content_css: spellCheckCss,

    });

       // check the spelling on a textarea
    $(".spellcheck").click(function (e) {
        standalonespellchecker = new $.SpellChecker(".checkspelling", {
            lang: spellcheckconfig.language,
            //this is for text field html is reserved for rich text
            parser: 'text',
            webservice: {
                //change to make configurable
                path: spellcheckconfig.handler,
                driver: spellcheckconfig.driver
            },
            suggestBox: {
                position: 'above',
                //appendTo: 'body',
                //position: positionSuggestBox(this)
            },
            incorrectWords: {
                //container: $(this).parent().find('#incorrect-word-list')
                container: '#incorrectwords'
            },
            getText: function () {
                return $('.checkspelling').val();
            }

        });
        standalonespellchecker.on('check.success', function () {
            alert('There are no incorrectly spelled words.');
        });
        standalonespellchecker.check();
        
    });

});
