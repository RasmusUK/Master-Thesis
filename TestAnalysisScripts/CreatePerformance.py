import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import argparse
import os
import sys

# Parse CLI arguments
parser = argparse.ArgumentParser()
parser.add_argument("file_path", help="Path to the CSV")
args = parser.parse_args()

# Validate file
if not os.path.isfile(args.file_path):
    print(f"Error: File not found: {args.file_path}")
    sys.exit(1)

# Load CSV
df = pd.read_csv(args.file_path, sep=";")

# Type conversions
df["EventStoreEnabled"] = df["EventStoreEnabled"].astype(bool)
df["EntityStoreEnabled"] = df["EntityStoreEnabled"].astype(bool)
df["PersonalDataStoreEnabled"] = df["PersonalDataStoreEnabled"].astype(bool)
df["Avg (ms)"] = df["Avg (ms)"].astype(str).str.replace(',', '.').astype(float)
df["Size (mb)"] = df["Size (mb)"].astype(str).str.replace(',', '.').astype(float)

# Normalize SizeName
df["SizeName"] = df["SizeName"].str.strip().str.capitalize()

# Compute consistent label values per SizeName
summary = (
    df.groupby("SizeName")[["Size (mb)", "Props", "Nodes"]]
    .mean()
    .round(3)
    .reset_index()
)

# Sort SizeNames by average size
summary = summary.sort_values("Size (mb)").reset_index(drop=True)

# Merge with main dataframe
df = df.merge(summary, on="SizeName", suffixes=("", "_Summary"))

# Consistent size label across same SizeName
df["SizeLabel"] = df.apply(
    lambda row: f"{row['SizeName']} ({row['Size (mb)_Summary']:.3f} MB, {int(row['Props_Summary'])} props, {int(row['Nodes_Summary'])} nodes)",
    axis=1
)

# Combine feature flag values
df["FeatureSet"] = df.apply(
    lambda row: f"Event:{row['EventStoreEnabled']} | Entity:{row['EntityStoreEnabled']} | Personal:{row['PersonalDataStoreEnabled']}",
    axis=1
)

# Final aggregation: true average per SizeLabel + FeatureSet
avg_df = df.groupby(["SizeLabel", "FeatureSet"], as_index=False)["Avg (ms)"].mean()

# Determine label order based on size sort
label_order = (
    df.drop_duplicates("SizeName")
    .sort_values("Size (mb)_Summary")["SizeLabel"]
    .tolist()
)

# Plot
plt.rc('font', size=11)

plt.figure(figsize=(14, 7))
barplot = sns.barplot(
    data=avg_df,
    x="SizeLabel",
    y="Avg (ms)",
    hue="FeatureSet",
    order=label_order,
    ci=None
)

plt.title("Average Repository Create Performance by Size (with MB, Props, Nodes) and Feature Flags", fontsize=14)
plt.ylabel("Average Time (ms)")
plt.xlabel("Size Category")
plt.xticks(rotation=30, ha='right')

# Add big, bold value labels
for p in barplot.patches:
    height = p.get_height()
    if not pd.isna(height) and height > 0:
        barplot.annotate(
            f'{height:.1f} ms',
            (p.get_x() + p.get_width() / 2., height),
            ha='center', va='bottom',
            fontsize=11,
            color='black', xytext=(0, 1),
            textcoords='offset points'
        )

# Legend in top-left inside plot
plt.legend(title="Feature Flags", loc='upper left', bbox_to_anchor=(0.01, 0.99))

plt.tight_layout()
plt.show()
