#!/bin/bash

# Constants
version_regex=".*[V|v]ersion to ([0-9\.]+(-((alpha)|(beta)))?)"
upload_url_regex=".*\"upload_url\" *: *\"(.*assets)\{\?name,label\}\""
assets_dir="${PWD}/publish" # Update assets directory here
access_token=$GITHUB_ACCESS_TOKEN # Update environment variable name set in CI here

# Functions
generate_post_data()
{
	cat <<EOF
	{
		"tag_name": "v$version",
		"target_commitish": "$branch",
		"name": "$(date +%Y%m%d)",
		"body": "$(get_commit_messages)",
		"draft": true,
		"prerelease": false
	}
EOF
}

get_commit_messages()
{
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
	echo "$(git log $previous_commit..HEAD~ --format=%B | sed '/^$/d' | sed -e 's/^/* /' | sed 's/\//\\\//g' | sed 's/\\/\\\\/g' | sed 's/\"/\\\"/g' | sed -E ':a;N;$!ba;s/\r{0,1}\n/\\n/g')"
}

success_banner()
{
	echo "======================================================================================================="
	echo "                                           Script Completed!                                           "
	echo "======================================================================================================="
	exit 0
}

failure_banner()
{
	echo "======================================================================================================="
	echo "                                            Script Failed!                                             "
	echo "======================================================================================================="
	exit 1
}

# Capture error.
set -e # Stop execution if any command fails, sto
trap "failure_banner" ERR # Run specified function when script stops due to failed command

# Script Starts Here
echo "======================================================================================================="
echo "                                         Github Release Script                                         "
echo "-------------------------------------------------------------------------------------------------------"
echo "                                   Copyright © Devil7Softwares 2020                                    "
echo "======================================================================================================="

# Read info from local repo
echo " - Reading info from local repository."
repo=$(git config --get remote.origin.url | sed 's/.*:\|@\/?\/?github.com\/\|://;s/.git$//') # Get origin url and split "<OWNER>/<REPO>"
branch=$(git rev-parse --abbrev-ref HEAD) # Get current branch
last_commit_message=$(git log -1 --pretty=%B) # Get last commit message

echo "   REPOSITORY     : $repo"
echo "   BRANCH         : $branch"
echo "   COMMIT MESSAGE : $last_commit_message"
echo ""

if [[ "$branch" != "master" ]]; then
	echo " - Branch is not master. Skipping..."
	success_banner
fi

if [[ $last_commit_message =~ $version_regex ]] ; then # Split version number from commit message
	version=${BASH_REMATCH[1]}

	# Create draft release. See https://developer.github.com/v3/repos/releases/#create-a-release
	echo " - Creating release draft for version v$version in github releases..."
	response=$(curl --fail --silent --data "$(generate_post_data)" "https://api.github.com/repos/$repo/releases?access_token=$access_token")

	if [[ $response =~ $upload_url_regex ]]; then # Parse response json and split upload url
		echo "   Success."
		echo " - Uploading Assets"
		upload_base_url=${BASH_REMATCH[1]}

		assets=$(find ./ -type f \( -iname \*.tar.gz -o -iname \*.zip \)) # Update assets filter here
		for file in $assets; do
			filename=$(basename "$file")
			mime_type=$(file -b --mime-type $file)

			upload_url="$upload_base_url?name=$filename&access_token=$access_token";
			echo -e "   Uploading : \"$filename\" [$mime_type]. \c"
			upload_response=$(curl --fail --silent --header "Content-Type:$mime_type" --data-binary "@$file" "$upload_url")
			echo "Success."
		done
		echo " - Finished creating release."
		success_banner
	else
		echo " - Unable to get upload url!"
		failure_banner
	fi
else
	echo " - Last commit message doesn't contain"
	echo "   version number. Skipping..."
	success_banner
fi