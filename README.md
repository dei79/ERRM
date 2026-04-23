# ERRM

## Styling workflow

Custom application styles are authored in SCSS and compiled into generated CSS files during the build.

- Global styles source: `ERRM/wwwroot/scss/site.scss`
- Generated global styles output: `ERRM/wwwroot/css/site.css`
- Razor scoped styles source: `ERRM/Views/Shared/_Layout.cshtml.scss`
- Generated Razor scoped output: `ERRM/Views/Shared/_Layout.cshtml.css`

Install dependencies with `npm install`, then use:

- `npm run build:scss` to compile SCSS once
- `npm run watch:scss` to rebuild styles while you work

`dotnet build` also compiles the SCSS automatically. The generated CSS files are not tracked in git.
