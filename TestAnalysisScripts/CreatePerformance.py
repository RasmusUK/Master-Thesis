import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import argparse
import os
import sys

# --- Parse CLI arguments ---
parser = argparse.ArgumentParser()
parser.add_argument("file_path", help="Path to the CSV")
args = parser.parse_args()

# --- Validate file ---
if not os.path.isfile(args.file_path):
    print(f"Error: File not found: {args.file_path}")
    sys.exit(1)

# --- Load CSV ---
df = pd.read_csv(args.file_path, sep=";")

# --- Type conversions ---
df["EventStoreEnabled"] = df["EventStoreEnabled"].astype(bool)
df["EntityStoreEnabled"] = df["EntityStoreEnabled"].astype(bool)
df["PersonalDataStoreEnabled"] = df["PersonalDataStoreEnabled"].astype(bool)
df["Size (mb)"] = df["Size (mb)"].astype(str).str.replace(',', '.').astype(float)
df["Time (ms)"] = df["Time (ms)"].astype(str).str.replace(',', '.').astype(float)

# --- Normalize SizeName ---
df["SizeName"] = df["SizeName"].str.strip().str.capitalize()

# --- Compute consistent labels per size category ---
summary = (
    df.groupby("SizeName")[["Size (mb)", "Props", "Nodes"]]
    .mean().round(3).reset_index()
)
summary = summary.sort_values("Size (mb)").reset_index(drop=True)
df = df.merge(summary, on="SizeName", suffixes=("", "_Summary"))

df["SizeLabel"] = df.apply(
    lambda row: f"{row['SizeName']} ({row['Size (mb)_Summary']:.3f} MB, {int(row['Props_Summary'])} props, {int(row['Nodes_Summary'])} nodes)",
    axis=1
)

df["FeatureSet"] = df.apply(
    lambda row: f"EventStore:{row['EventStoreEnabled']} | EntityStore:{row['EntityStoreEnabled']} | PersonalDataStore:{row['PersonalDataStoreEnabled']}",
    axis=1
)

# --- Sort SizeLabels ---
label_order = (
    df.drop_duplicates("SizeName")
    .sort_values("Size (mb)_Summary")["SizeLabel"]
    .tolist()
)

# --- Plot with error bars (standard deviation) ---
plt.rc('font', size=11)
plt.figure(figsize=(16, 7))

barplot = sns.barplot(
    data=df,
    x="SizeLabel",
    y="Time (ms)",
    hue="FeatureSet",
    order=label_order,
    capsize=0.1,
    ci="sd"
)

for line in barplot.lines:
    line.set_alpha(0.4)  # adjust for translucency


plt.title("Repository Create Performance by Size (with MB, Props, Nodes) and Feature Flags", fontsize=14)
plt.ylabel("Time (ms)")
plt.xlabel("Size Category")
plt.xticks(rotation=30, ha='right')
plt.legend(title="Feature Flags", loc='upper left', bbox_to_anchor=(0.01, 0.99))

for i, p in enumerate(barplot.patches):
    height = p.get_height()
    if not pd.isna(height) and height > 0:
        label = f"{int(height)}" if height >= 100 else f"{height:.1f}"
        barplot.annotate(
            label,
            (p.get_x() + p.get_width() / 2., height),
            ha='center', va='bottom',
            color='black', xytext=(0, 0),
            textcoords='offset points'
        )

plt.tight_layout()
plt.show()
