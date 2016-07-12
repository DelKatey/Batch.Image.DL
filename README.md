## Introduction

Do you like "archiving" data? Especially ones that tends to be ordered sequentially numerically? And does those data tend to be images of a sort? Fret no more!

![The program upon starting](/Screenshots/main.png)

## Usage

First, you need to get an URL that the program can work off of. Currently, the program is designed to work with two different types of URL that are very similar. They are:

- MangaHere (example: http://www.mangahere.co/manga/koko_ni_iru_yo/v01/c001/43.html)
- MangaFox (example: http://mangafox.me/manga/koko_ni_iru_yo/v01/c001/3.html)

The above URLS can be obtained by right-clicking an image, then selecting "View Image" in Firefox, or "Open image in new tab" in Chrome.

After getting the URL, feed it into the "Main URL" field, then provide the desired starting and ending page numbers. This can be, for example, "2" and "43" respectively, if using the above examples, but can also be any number in between, so long as the ending page number is always higher than the starting page number.

 The starting page number always has to be filled no matter which mode you are using the program in, but the ending page number is only needed for the Batch modes.

 The Preview modes, as of now, just streams the images, whereas the Download modes will stream, and then subsequently save the image streams to a designated path. In the future, this may very well change.
 
## Notes

This version has been completely reworked, using a different method to get the images, with much more compatibility across various different image naming schemes. Due to that, its speed is now slower, until a way has been found to speed up the process without compromising the compatability improvements.
 
## Installation

This was coded in C#, .NET Framework 4, with Visual Studio 2010. This repository contains the complete files that constitutes a working version of the program.

## License

This project is licensed under the [GNU General Public License v3](LICENSE.md).