# syntax=docker/dockerfile:1

ARG ORIGAM_SERVER_IMAGE
FROM ${ORIGAM_SERVER_IMAGE}

COPY --chown=origam:origam model/ /home/origam/projectData/origam/model/
COPY --chown=origam:origam customAssets/ /home/origam/projectData/origam/customAssets/
COPY --chown=origam:origam l10n/ /home/origam/projectData/origam/l10n/
