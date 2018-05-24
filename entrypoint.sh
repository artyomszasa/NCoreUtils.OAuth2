#!/bin/bash

if [ -z "$DB_HOST" ]; then
    echo 'No database host specified' >&2
    exit -1
fi

if [ -z "$DB_PORT" ]; then
    echo 'No database port spcified, assuming 5432'
    DB_PORT=5432
fi

if [ -z "$DB_DATABASE" ]; then
    echo 'No database port spcified, assuming "ncoreutils-oauth2"'
    DB_PORT=ncoreutils-oauth2
fi

if [ -z "$DB_USER" ]; then
    echo 'No database user spcified, assuming ncoreutils'
    DB_USER=ncoreutils
fi

if [ -z "$DB_PASSWORD" ]; then
    echo 'No database password specified' >&2
    exit -1
fi

if [ -z "$GOOGLE_PROJECTID" ]; then
    GOOGLE_PROJECTID=$(curl "http://metadata.google.internal/computeMetadata/v1/project/project-id" -H "Metadata-Flavor: Google")
    if [ -z "$GOOGLE_PROJECTID" ]; then
        echo 'Unable to retreive project id. When using outside google environment GOOGLE_PROJECTID must be defined!' >&2
        exit -1
    fi
    echo "Using $GOOGLE_PROJECTID as Google project id"
fi

if [ -z "$GOOGLE_LOCATIONID" ]; then
    GOOGLE_LOCATIONID=$(curl -s "http://metadata.google.internal/computeMetadata/v1/instance/zone" -H "Metadata-Flavor: Google" | egrep -o '(europe|us|asia)-([a-z]+[0-9])')
    if [ -z "$GOOGLE_LOCATIONID" ]; then
        echo 'Unable to retreive location id. When using outside google environment GOOGLE_LOCATIONID must be defined!' >&2
        exit -1
    fi
    echo "Using $GOOGLE_LOCATIONID as Google location id"
fi

if [ -z "$GOOGLE_KEYRINGID" ]; then
    echo 'No KMS keyring id specified' >&2
    exit -1
fi

if [ -z "$GOOGLE_KEYID" ]; then
    echo 'No KMS key id specified' >&2
    exit -1
fi


cat /app/appsettings.json.template |\
    sed -e "s/GOOGLE_PROJECTID/$GOOGLE_PROJECTID/g" |\
    sed -e "s/GOOGLE_LOCATIONID/$GOOGLE_LOCATIONID/g" |\
    sed -e "s/GOOGLE_KEYRINGID/$GOOGLE_KEYRINGID/g" |\
    sed -e "s/GOOGLE_KEYID/$GOOGLE_KEYID/g" |\
    sed -e "s/DB_HOST/$DB_HOST/g" |\
    sed -e "s/DB_PORT/$DB_PORT/g" |\
    sed -e "s/DB_DATABASE/$DB_DATABASE/g" |\
    sed -e "s/DB_USER/$DB_USER/g" |\
    sed -e "s/DB_PASSWORD/$DB_PASSWORD/g" > /app/appsettings.json

/app/NCoreUtils.OAuth2.WebService --tcp=0.0.0.0:80
