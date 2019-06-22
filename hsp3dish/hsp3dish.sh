patch -u < hsp3dish.patch
mv -f ./breakblock.html ./app/BreakBlock.html
mv -f ./breakblock.data ./app/BreakBlock.data
mv -f ./hsp3dish.js ./app/hsp3dish.js
rm -f ./breakblock.ax
