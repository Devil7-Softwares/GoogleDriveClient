#!/bin/bash

# get_commit_messages()
# {
	# Finds the id of a commit that matches "Bump Version to *.*.*" commit message skipping last one
	previous_commit=$(git rev-list HEAD~ --grep=".*[V|v]ersion to .*")

	# If there is no commit that matches the above condition, find the initial commit id.
	if [ -z "$previous_commit" ];then
		previous_commit=$(git rev-list HEAD --max-parents=0)
	fi

	# Lists all commit messages from $previous_commit to HEAD~
	#
	# sed '/^$/d'									=	Removes all empty lines
	# sed -e 's/^/* /'								=	Appends asterisk (*) to each line
	# sed 's/\//\\\//g'								=	Escaps forward slash i.e. '/' to '\/'
	# sed 's/\\/\\\\/g'								=	Escaps backward slash i.e '\' to '\\'
	# sed 's/\"/\\\"/g'								=	Escaps double quotes (")
	# sed -E ':a;N;$!ba;s/\r{0,1}\n/\\n/g'			=	Replaces newline charecter with newline litral i.e. '<newline>' to '\n'
	#
	# All the above filters are required to keep the json payload valid.
	message=$(git log $previous_commit..HEAD~ --format=%B | sed '/^$/d')

	echo "CHANGELOG<<EOF" >> $GITHUB_OUTPUT
	# Loop through each line and append it to file
	while read -r line; do
		# Skip if string contains "Bump version to .*" or "[skip ci]"
		if [[ $line =~ ^Bump\ version\ to\ .* ]] || [[ $line =~ ^\[skip\ ci\] ]]; then
			continue
		fi

	 	echo "* $line" >> $GITHUB_OUTPUT
	done <<< "$message"
	echo "EOF" >> $GITHUB_OUTPUT

	echo "TITLE=$(date +%Y%m%d)" >> $GITHUB_OUTPUT
# }

# CHANGELOG="$(get_commit_messages)"
# echo $CHANGELOG
# echo "CHANGELOG=$CHANGELOG" >> $GITHUB_OUTPUT