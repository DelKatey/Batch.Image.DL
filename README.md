## Introduction

Do you like "archiving" data? Especially ones that tends to be ordered sequentially numerically? And does those data tend to be images of a sort? Fret no more!

![The program upon starting](\Screenshots\main.png)

## Usage

First, you need to get an URL that the program can work off of. Currently, the program is designed to work with two different types of URL that are very similar. They are:

- MangaHere (example: http://a.mhcdn.net/store/manga/13739/134.0/compressed/r001.jpg?v=1468230242)
- MangaFox (example: http://z.mfcdn.net/store/manga/13088/03-134.0/compressed/g001.jpg)

The above URLS can be obtained by right-clicking an image, then selecting "View Image" in Firefox, or "Open image in new tab" in Chrome.

After getting the URL, feed it into the "Main URL" field, then provide the desired starting and ending page numbers. This can be, for example, "1" and "21" respectively, if using the above examples, but can also be any number in between, so long as the ending page number is always higher than the starting page number.

 The starting page number always has to be filled no matter which mode you are using the program in, but the ending page number is only needed for the Batch modes.

 The Preview modes, as of now, just streams the images, whereas the Download modes will stream, and then subsequently save the image streams to a designated path. In the future, this may very well change.
 
## Installation

This was coded in C#, .NET Framework 4, with Visual Studio 2010. This repository contains the complete files that constitutes a working version of the program.

## License

This project is licensed under the [GNU General Public License v3](LICENSE.md).