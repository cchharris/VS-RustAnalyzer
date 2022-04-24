# VS-RustAnalyzer

[![.NET](https://github.com/cchharris/VS-RustAnalyzer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cchharris/VS-RustAnalyzer/actions/workflows/dotnet.yml)  ![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/cchharris/VS-RustAnalyzer?include_prereleases)

Source Repository for [VS_RustAnalyzer](https://marketplace.visualstudio.com/items?itemName=cchharris.vsrustanalyzer) Visual Studio Package.

## Overview

This is an *unofficial* extension, with the aim of working with Rust in Visual Studio as great an experience as C# and C++.  We use Visual Studio's language server code to interact with [rust-analyzer](https://rust-analyzer.github.io/), an excellent language server implementation.

## Installation

This extension assumes you have already [installed rust-analyze](https://rust-analyzer.github.io/manual.html#rust-analyzer-language-server-binary).  It will look for `rust-analyzer.exe` on your `PATH`.  Alternatively, you can open Tools->Rust Analyzer and give the full path to `rust-analyzer.exe`, and it should work with a restart.

## Features
### Language Server
As Visual Studio is still implementing and improving language server features, this plugin supports the cross-section of features from [Visual Studio](https://docs.microsoft.com/en-us/visualstudio/extensibility/adding-an-lsp-extension?view=vs-2022) and [rust-analyzer](https://github.com/rust-lang/rust-analyzer).

### Visual Studio Specific
_WORK IN PROGRESS_

#### Implemented

[x] Toml language grammar

[x] File icons (placeholder)

[x] Build

#### In Progress

[x] Startup Options - Basic bin support

#### Future

[_] Test Support

[_] Project Setup

[_] Better Cargo.toml feature set support
 * workspaces
 * [config.toml](https://doc.rust-lang.org/cargo/reference/config.html)
