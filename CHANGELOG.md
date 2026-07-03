# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-07-03

### Added

- Added overload for keeping secrets segregated: `AddSegregatedApiSecretRepository<TSegregatedApiSecret, TKey, TRepository>` as well as the equivalent `AddSegregatedApiSecretMongoRepository<TSegregatedApiSecret, TKey>` for MongoDB.

## [1.1.0] - 2026-03-20

### Fixed

- Made DI more robust against misconfiguration when using custom `TApiSecret` by chaining methods onto a custom service collection.

## [1.0.0] - 2026-03-15

### Added

- First version