# Flaneer Streaming App

This repo contains all code related to the Flaneer streaming technology, the low level capture and encoding as well as the application code.

## Coding style

For C# follow the MS standards, except around naming with underscores, importantly:

- Use pascal casing ("PascalCasing") when naming a class, record, or struct.
- Use camel casing ("camelCasing") when naming private or internal fields.

https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions#additional-naming-conventions

For C++ follow the same style, however these addition rules apply for the C++ specific language features:

http://web.mit.edu/6.s096/www/standards.html

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