# Molecularity Web

The production site is fully static and lives in `wwwroot`. It can be served locally with:

```bash
python3 -m http.server 8080 --directory wwwroot
```

## GitHub Pages

Install the prepared repository-level workflow once:

```bash
./install-pages-workflow.sh
```

Commit and push the changes to `main`. In the GitHub repository settings, open
**Pages** and select **GitHub Actions** as the deployment source. Every subsequent push
to `main` deploys `wwwroot` automatically.

The browser version does not require ASP.NET hosting. Its rules mirror
`Molecularity.Core`, and the canonical level JSON files are bundled under
`wwwroot/levels`.
