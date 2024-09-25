rec {
  description = "Altibiz.DependencyInjection.Extensions";

  inputs = {
    flake-utils.url = "github:numtide/flake-utils";
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-24.11";
  };

  outputs = { self, nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config = { allowUnfree = true; };
          overlays = [
            (final: prev: {
              dotnet-sdk = prev.dotnet-sdk_8;
              dotnet-runtime = prev.dotnet-runtime_8;
            })
          ];
        };
      in
      {
        devShells.deploy = pkgs.mkShell {
          packages = with pkgs; [
            # Scripts
            just
            nushell

            # C#
            dotnet-sdk
            dotnet-runtime
          ];
        };
        devShells.docs = pkgs.mkShell {
          packages = with pkgs; [
            # Scripts
            just
            nushell

            # C#
            dotnet-sdk
            dotnet-runtime

            # Documentation
            mdbook
          ];
        };
        devShells.check = pkgs.mkShell {
          packages = with pkgs; [
            # Scripts
            just
            nushell

            # Nix
            nixpkgs-fmt

            # C#
            dotnet-sdk
            dotnet-runtime

            # Markdown
            markdownlint-cli
            nodePackages.markdown-link-check

            # Spelling
            nodePackages.cspell

            # Misc
            nodePackages.prettier
          ];
        };
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            # Version Control
            git

            # Scripts
            just
            nushell

            # Nix
            nil
            nixpkgs-fmt

            # C#
            dotnet-sdk
            dotnet-runtime
            omnisharp-roslyn
            netcoredbg

            # Markdown
            marksman
            markdownlint-cli
            nodePackages.markdown-link-check

            # Spelling
            nodePackages.cspell

            # Documentation
            mdbook

            # Misc
            nodePackages.prettier
            nodePackages.yaml-language-server
            nodePackages.vscode-langservers-extracted
            taplo
          ];
        };

        packages.default = pkgs.buildDotnetModule {
          pname = "Altibiz.DependencyInjection.Extensions";
          version = "0.1.0";

          src = self;
          projectFile = "src/Altibiz.DependencyInjection.Extensions/Altibiz.DependencyInjection.Extensions.csproj";
          nugetDeps = ./deps.nix;

          dotnet-sdk = pkgs.dotnet-sdk;
          dotnet-runtime = pkgs.dotnet-runtime;

          meta = {
            description = description;
            homepage = "https://github.com/altibiz/extensions-dependency-injection";
            license = pkgs.lib.licenses.mit;
          };
        };
      });
}
