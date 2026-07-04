# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-07-04

### Added

- Added overload for keeping secrets segregated. Use the `Segregated` method overloads when adding Authentication or the Provider and you'll get equivalent `Segregated` methods for adding your repository, which is where the segregation
  takes place.
- Updated README to showcase data segregation.

## [1.1.0] - 2026-03-20

### Fixed

- Made DI more robust against misconfiguration when using custom `TApiSecret` by chaining methods onto a custom service collection.

## [1.0.0] - 2026-03-15

### Added

- First version