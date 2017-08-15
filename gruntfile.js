module.exports = function(grunt) {
  require('load-grunt-tasks')(grunt);
  var path = require('path')

  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    pkgMeta: grunt.file.readJSON('config/meta.json'),
    dest: grunt.option('target') || 'dist',
    basePath: path.join('<%= dest %>', '<%= pkgMeta.name %>'),
    copy: {
      dll: {
        cwd: 'src/Machina/bin',
        src: 'Machina.dll',
        dest: '<%= dest %>/bin/',
        expand: true
      },
      nuget: {
        files: [
          {
            cwd: 'src/Machina/bin/debug',
            src: ['Machina.dll'],
            dest: 'tmp/nuget/lib/net45',
            expand: true
          }
        ]
      }
    },
    nugetpack: {
        dist: {
            src: 'tmp/nuget/package.nuspec',
            dest: 'pkg'
        }
    },
    template: {
        'nuspec': {
            'options': {
                'data': { 
                    name: '<%= pkgMeta.name %>',
                    version: '<%= pkgMeta.version %>',
                    url: '<%= pkgMeta.url %>',
                    license: '<%= pkgMeta.license %>',
                    licenseUrl: '<%= pkgMeta.licenseUrl %>',
                    author: '<%= pkgMeta.author %>',
                    authorUrl: '<%= pkgMeta.authorUrl %>',
                    files: [{ path: 'tmp/nuget/content/App_Plugins', target: 'content/App_Plugins'}]
                }
            },
            'files': { 
                'tmp/nuget/package.nuspec': ['config/package.nuspec']
            }
        }
    },
    clean: {
      build: '<%= grunt.config("basePath").substring(0, 4) == "dist" ? "dist/**/*" : "null" %>',
      tmp: ['tmp']
    },
    assemblyinfo: {
      options: {
        files: ['src/Machina/Machina.csproj'],
        filename: 'AssemblyInfo.cs',
        info: {
          version: '<%= (pkgMeta.version.indexOf("-") ? pkgMeta.version.substring(0, pkgMeta.version.indexOf("-")) : pkgMeta.version) %>', 
          fileVersion: '<%= pkgMeta.version %>'
        }
      }
    },
    msbuild: {
      options: {
        stdout: true,
        verbosity: 'quiet',
        maxCpuCount: 4,
        version: 4.0,
        buildParameters: {
          WarningLevel: 2,
          NoWarn: 1607
        }
      },
      dist: {
        src: ['src/Machina/Machina.csproj'],
        options: {
          projectConfiguration: 'Debug',
          targets: ['Clean', 'Rebuild'],
        }
      }
    }
  });

  grunt.registerTask('default', ['clean', 'assemblyinfo', 'copy:dll']);
  grunt.registerTask('nuget',   ['clean:tmp', 'default', 'copy:nuget', 'template:nuspec', 'nugetpack']);
};