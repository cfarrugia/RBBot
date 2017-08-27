module.exports = function (grunt) {
    grunt.initConfig({
        bower: {
            install: {
                options: {
                    targetDir: "wwwroot/js",
                    layout: "byComponent",
                    cleanTargetDir: false
                }
            }
        },
        
    });
    grunt.registerTask("default", ["bower:install"]);
    grunt.loadNpmTasks("grunt-bower-task");
    
};