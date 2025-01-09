set windows-shell := ["nu.exe", "-c"]
set shell := ["nu", "-c"]

root := absolute_path('')
sln := absolute_path('extensions-dependency-injection.sln')
gitignore := absolute_path('.gitignore')
prettierignore := absolute_path('.prettierignore')
jbcache := absolute_path('.jb/cache')
jbcleanuplog := absolute_path('.jb/cleanup.log')
jbinspectlog := absolute_path('.jb/inspect.log')
artifacts := absolute_path('artifacts')
src := absolute_path('src')
docs := absolute_path('docs')

default:
    @just --choose

prepare:
    dotnet tool restore
    (not (which prettier | is-empty)) or (npm install -g prettier) | ignore

ci:
    dotnet tool restore

format:
    cd '{{ root }}'; just --fmt --unstable

    nixpkgs-fmt '{{ root }}'

    prettier --write \
      --ignore-path '{{ gitignore }}' \
      --ignore-path '{{ prettierignore }}' \
      --cache --cache-strategy metadata \
      '{{ root }}'

    dotnet jb cleanupcode '{{ sln }}' \
      --verbosity=WARN \
      --caches-home='{{ jbcache }}' \
      -o='{{ jbinspectlog }}' \
      --exclude='**/.git/**/*;**/.nuget/**/*;**/obj/**/*;**/bin/**/*'

deps:
    exec \
      (nix build ".#default.fetch-deps" --print-out-paths --no-link) \
      deps.nix

lint:
    prettier --check \
      --ignore-path '{{ gitignore }}' \
      --ignore-path '{{ prettierignore }}' \
      --cache --cache-strategy metadata \
      '{{ root }}'

    cspell lint '{{ root }}' \
      --no-progress

    markdownlint '{{ root }}'
    markdown-link-check \
      --config .markdown-link-check.json \
      --quiet \
      ...(glob '**/*.md')

    dotnet build --no-incremental /warnaserror '{{ sln }}'

    dotnet roslynator analyze '{{ sln }}' \
      --exclude='**/.git/**/*;**/.nuget/**/*;**/obj/**/*;**/bin/**/*'

    dotnet jb inspectcode '{{ sln }}' \
      --no-build \
      --verbosity=WARN \
      --caches-home='{{ jbcache }}' \
      -o='{{ jbinspectlog }}' \
      --exclude='**/.git/**/*;**/.nuget/**/*;**/obj/**/*;**/bin/**/*'

test *args:
    dotnet test '{{ sln }}' {{ args }}

docs:
    rm -rf '{{ artifacts }}'
    mkdir '{{ artifacts }}'

    dotnet docfx metadata '{{ docs }}/code/docfx.json'
    dotnet docfx build '{{ docs }}/code/docfx.json'
    cp -f '{{ docs }}/favicon.png' '{{ artifacts }}/code'

    mdbook build '{{ docs }}/wiki'
    mv '{{ docs }}/wiki/book' '{{ artifacts }}/wiki'

    cp '{{ docs }}/index.html' '{{ artifacts }}'
    cp '{{ docs }}/favicon.png' '{{ artifacts }}'

deploy api_key version:
    rm -rf '{{ artifacts }}'
    mkdir '{{ artifacts }}'

    dotnet pack '{{ src }}/Altibiz.DependencyInjection.Extensions' \
      --configuration Release \
      -p:PackageVersion={{ version }} \
      -p:RepositoryUrl=https://github.com/altibiz/extensions-dependency-injection \
      -o '{{ artifacts }}'

    dotnet nuget push \
      {{ artifacts }}/*.nupkg \
      --api-key {{ api_key }} \
      --source https://api.nuget.org/v3/index.json
