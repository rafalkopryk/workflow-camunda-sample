# Cockpit history plugin

`operaton-webapp-webjar-2.1.4.jar` is the Operaton 2.1.4 webapp webjar with
the Apache-2.0 `datakurre/operaton-cockpit-plugins` bundles added. The plugin
source is pinned to commit `a68322ec1aa1557b961f6bb6ba133d71add49c01`.

Aspire bind-mounts this file over the matching webjar in the official
`operaton/operaton:2.1.4` container. No custom container image is built.
