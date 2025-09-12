const pkg = require('../frontend/package.json');
const ver = pkg.devDependencies['@angular/cli'] || '';
const major = ver.replace(/[^0-9.]/g, '').split('.')[0];
if (major !== '19') {
  console.error('Angular CLI 19 required');
  process.exit(1);
}
console.log('Angular CLI version OK:', ver);
