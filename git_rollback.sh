#!/bin/bash
git branch backup-before-rollback
git reset --hard origin/develop
git cherry-pick b40edd3
git cherry-pick c62d577
git cherry-pick ff30f6b
