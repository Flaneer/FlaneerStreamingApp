# Flaneer Streaming App

This repo contains all code related to the Flaneer streaming technology, the low level capture and encoding as well as the application code.

## Coding style

In general the following guidelines should be followed:
- Commented out sections of code should not be in master (that is what Git is for)
- There should be zero warnings
    - Warnings that are decided to be acceptable should be commented using a pragma

For C# follow the MS standards, with some changes, importantly:

- Use camel casing ("camelCasing") when naming private or internal fields.
- All caps for enum names

https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#additional-naming-conventions

For C++ follow the same style, however these additional rules apply for the C++ specific language features:

- Those listed here:
    - http://web.mit.edu/6.s096/www/standards.html
- Use `m_` for members of a class, this is because the header is a seperate file. No other form of Hungarian notation should be used.
- 

## Git rules

Branches *should* be named after linear tickets where possible to enable us to use the automatic status detection, however some branches may contain multiple tickets work, in which case just selecting one ticket and then mentioning the others in the relavent commits is fine. (If there is not a Linear ticket for the work you are about to undertake... there should be!)

There is no direct pushing to master, all work should go through the standard PR process.

## Useful resources

- Initial research for streaming, there are some useful links on here:
    - https://miro.com/app/board/uXjVOWgIaFs=/?share_link_id=153011960191
- Initial designs for the streaming app (this should be in date, but will likely lack relavence over time):
    - https://miro.com/app/board/uXjVORPp6F8=/?share_link_id=193081114046
- This guide on the group of pictures from Amazon contains some really useful video basics:
    - https://aws.amazon.com/blogs/media/part-1-back-to-basics-gops-explained/
- Raw video test footage:
    - https://media.xiph.org/video/derf/