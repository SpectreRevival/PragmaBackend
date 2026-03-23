#include "playtab.h"

#include "ui_PlayTab.h"
#include <QPropertyAnimation>
#include <QDirIterator>

constexpr static int CAROUSEL_ANIMATION_DURATION = 5000;
static QEasingCurve CAROUSEL_EASING_CURVE = QEasingCurve::InOutQuad;

PlayTab::PlayTab(QWidget* parent) : QWidget(parent), ui(new Ui::PlayTab) {
    ui->setupUi(this);
    QDirIterator it(":/assets/carousel", QDirIterator::NoIteratorFlags);
    while (it.hasNext()) {
        carouselImages.push_back(new QPixmap(it.next()));
    }
    StartCarouselAnimation();
}

static void UpdateLabelToImage(QLabel* label, QPixmap* pixmap) {
    label->setPixmap(pixmap->scaled(label->size(), Qt::KeepAspectRatio, Qt::SmoothTransformation));
}

void PlayTab::StartCarouselAnimation() {
    UpdateLabelToImage(ui->Carousel1, carouselImages.at(currentCarouselIndex));
    UpdateLabelToImage(ui->Carousel2, carouselImages.at(currentCarouselIndex + 1 < carouselImages.size() ? currentCarouselIndex + 1 : 0));
    QPropertyAnimation* anim1 = new QPropertyAnimation(ui->Carousel1, "pos");

    QPoint startPos = ui->Carousel1->pos();
    QPoint endPos = QPoint(startPos.x() - ui->Carousel1->width(), startPos.y());
    anim1->setStartValue(startPos);
    anim1->setEndValue(endPos);
    anim1->setDuration(CAROUSEL_ANIMATION_DURATION);
    anim1->setEasingCurve(CAROUSEL_EASING_CURVE);
    anim1->start(QAbstractAnimation::DeleteWhenStopped);

    QPropertyAnimation* anim2 = new QPropertyAnimation(ui->Carousel2, "pos");
    QPoint startPos2 = ui->Carousel2->pos();
    QPoint endPos2 = QPoint(startPos2.x() - ui->Carousel2->width(), startPos2.y());
    anim2->setStartValue(startPos2);
    anim2->setEndValue(endPos2);
    anim2->setDuration(CAROUSEL_ANIMATION_DURATION);
    anim2->setEasingCurve(CAROUSEL_EASING_CURVE);
    anim2->start(QAbstractAnimation::DeleteWhenStopped);
}

PlayTab::~PlayTab() {
    delete ui;
    for (QPixmap* carouselImage : carouselImages) {
        delete carouselImage;
    }
}
