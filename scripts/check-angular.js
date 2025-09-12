#!/usr/bin/env node
const fs = require('fs');
const pkg = JSON.parse(fs.readFileSync('frontend/package.json', 'utf8'));
const ver = pkg.devDependencies['@angular/cli'] || pkg.dependencies['@angular/cli'];
const major = parseInt((ver.match(/\d+/)||['0'])[0],10);
if (major !== 19) {
  console.error(`Angular CLI major version ${major} detected, expected 19`);
  process.exit(1);
} else {
  console.log(`Angular CLI version ${ver} OK`);
}
