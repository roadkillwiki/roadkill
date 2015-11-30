/// <binding AfterBuild='default' />
module.exports = function (grunt) {

	// The installer css/js files are referenced from the Typescript output files, and not combined/minified.

    grunt.initConfig({

        sass: {
            dist: {
                options: {
                    style: 'compressed'
                },
                files: {
                	'Assets/CSS/roadkill.css': 'Assets/CSS/roadkill.scss',
					'Assets/CSS/roadkill.installer.css': 'Assets/CSS/roadkill.installer.scss'
                }
            }
        },	
        concat: {
        	all: {
        		src: [
						'Assets/Scripts/jquery/jquery-1.9.1.js',
						'Assets/Scripts/jquery/jquery-ui-1.10.3.custom.js',
						'Assets/Scripts/jquery/jquery.fieldSelection.js',
						'Assets/Scripts/jquery/jquery.fileupload.js',
						'Assets/Scripts/jquery/jquery.form-extensions.js',
						'Assets/Scripts/jquery/jquery.iframe-transport.js',
						'Assets/Scripts/jquery/jquery.timeago.js',
						'Assets/Scripts/jquery/jquery.validate.js',
						'Assets/Scripts/jquery/additional-methods.js',
						'Assets/Scripts/roadkill/dialogs.js',
						'Assets/Scripts/roadkill/setup.js',
						'Assets/Scripts/roadkill/validation.js',
						'Assets/Scripts/roadkill/editpage/editpage.js',
						'Assets/Scripts/roadkill/editpage/wysiwygeditor.js',
						'Assets/Scripts/roadkill/filemanager/ajaxrequest.js',
						'Assets/Scripts/roadkill/filemanager/breadcrumbtrail.js',
						'Assets/Scripts/roadkill/filemanager/buttonevents.js',
						'Assets/Scripts/roadkill/filemanager/htmlbuilder.js',
						'Assets/Scripts/roadkill/filemanager/setup.js',
						'Assets/Scripts/roadkill/filemanager/tableevents.js',
						'Assets/Scripts/roadkill/filemanager/util.js',
						'Assets/Scripts/roadkill/sitesettings/settings.js',
						'Assets/Scripts/shared/bootbox.js',
						'Assets/Scripts/shared/head.js',
						'Assets/Scripts/shared/tagmanager.js',
						'Assets/Scripts/shared/toastr.js'
        		],
        		dest: 'Assets/Scripts/roadkill.js'
        	}
        },
        uglify: {
        	all: {
        		src: ['Assets/Scripts/roadkill.js'],
        		dest: 'Assets/Scripts/roadkill.min.js'
        	}
        },
    });

    grunt.registerTask("default", ["sass", "concat", "uglify"]);

    grunt.loadNpmTasks("grunt-bower-task");
    grunt.loadNpmTasks("grunt-contrib-sass");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-contrib-concat");
    grunt.loadNpmTasks("grunt-contrib-uglify");
};