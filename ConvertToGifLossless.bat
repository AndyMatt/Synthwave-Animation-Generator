ffmpeg -framerate 50 -start_number 1 -i ScreenShots%%04d.png -vf palettegen palettegen.png
ffmpeg -framerate 50 -start_number 1 -i ScreenShots%%04d.png -i palettegen.png -lavfi paletteuse out.gif
del palettegen.png