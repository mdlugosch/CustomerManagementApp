process.once('beforeExit', function () {
    eval(process.argv[2]);
});

var fs = require('fs.extra')

console.log('Starting script: npmfile!'); function copyLib() {
    fs.copyRecursive('./node_modules/bootstrap/dist/css',
        './wwwroot/lib/bootstrap/dist/css',
        function (err) {
            if (err) {
                console.error(err);
            }
            else {
                console.log("success!");
            }
        }
    );
}